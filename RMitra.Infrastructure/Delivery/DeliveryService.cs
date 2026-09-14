using Dapper;
using Microsoft.Extensions.Options;
using RMitra.Application.Abstractions;
using RMitra.Application.Delivery;
using RMitra.Application.Identity;
using RMitra.Application.Settlement;
using RMitra.BuildingBlocks.Exceptions;
using RMitra.Domain.Common;
using RMitra.Domain.Delivery;
using RMitra.Infrastructure.Identity;
using RMitra.Infrastructure.Options;
using RMitra.Infrastructure.Ordering;

namespace RMitra.Infrastructure.Delivery;

public class RiderService : IRiderService
{
    private readonly ISqlConnectionFactory _connections;
    private readonly IPublicIdGenerator _ids;
    private readonly IPasswordHasher _passwords;
    private readonly ITokenService _tokens;
    private readonly IGeoCalculator _geo;
    private readonly IdentityService _identity;
    private readonly ISettlementService _settlements;
    private readonly CommerceOptions _commerce;

    public RiderService(
        ISqlConnectionFactory connections,
        IPublicIdGenerator ids,
        IPasswordHasher passwords,
        ITokenService tokens,
        IGeoCalculator geo,
        IdentityService identity,
        ISettlementService settlements,
        IOptions<CommerceOptions> commerce)
    {
        _connections = connections;
        _ids = ids;
        _passwords = passwords;
        _tokens = tokens;
        _geo = geo;
        _identity = identity;
        _settlements = settlements;
        _commerce = commerce.Value;
    }

    public async Task<RegisterRiderResponse> RegisterAsync(RegisterRiderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.VehicleType is not "BIKE" and not "SCOOTER" and not "CYCLE")
            throw AppException.Validation("vehicleType must be BIKE, SCOOTER, or CYCLE.");

        _passwords.ValidateStrength(request.Password);
        using var db = _connections.Create();
        var otp = await _identity.RequireFreshToken(db, request.VerificationToken);
        if (!string.Equals(otp.Purpose, "RIDER", StringComparison.OrdinalIgnoreCase))
            throw AppException.Forbidden("OTP purpose must be RIDER.");

        if (await IdentityService.FindUser(db, otp.MobileNumber, "RIDER") is not null)
            throw AppException.Conflict("Rider already exists for this mobile.", "RIDER_ALREADY_EXISTS");

        var user = await _identity.InsertUser(db, otp.MobileNumber, "RIDER", request.Name.Trim(), _passwords.Hash(request.Password), null);
        var rider = new Rider
        {
            Id = Guid.NewGuid(),
            RiderId = await _ids.NextAsync(PublicIdPrefixes.Rider),
            UserId = user.Id,
            FullName = request.Name.Trim(),
            MobileNumber = otp.MobileNumber,
            VehicleType = request.VehicleType,
            Status = RiderStatuses.Draft,
            AccountHolderName = request.BankDetails?.Holder,
            AccountNumber = request.BankDetails?.Account,
            IfscCode = request.BankDetails?.Ifsc,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await db.ExecuteAsync(
            @"INSERT INTO delRiders
                (Id, RiderId, UserId, FullName, MobileNumber, VehicleType, Status, AccountHolderName, AccountNumber, IfscCode, CreatedAt, UpdatedAt)
              VALUES
                (@Id, @RiderId, @UserId, @FullName, @MobileNumber, @VehicleType, @Status, @AccountHolderName, @AccountNumber, @IfscCode, @CreatedAt, @UpdatedAt)",
            rider);

        return new RegisterRiderResponse
        {
            RiderId = rider.RiderId,
            AccessToken = _tokens.CreateAccessToken(user),
            ExpiresAt = _tokens.GetExpiryUtc(),
            User = UserProfileDto.From(user)
        };
    }

    public async Task AddDocumentAsync(Guid userId, string documentType, string fileUrl, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rider = await RequireByUser(db, userId);
        await db.ExecuteAsync(
            "INSERT INTO delRiderDocuments (Id, RiderGuid, DocumentType, FileUrl, CreatedAt) VALUES (@Id, @RiderGuid, @DocumentType, @FileUrl, SYSUTCDATETIME())",
            new { Id = Guid.NewGuid(), RiderGuid = rider.Id, DocumentType = documentType, FileUrl = fileUrl });
    }

    public async Task SubmitAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rider = await RequireByUser(db, userId);
        var types = (await db.QueryAsync<string>("SELECT DocumentType FROM delRiderDocuments WHERE RiderGuid=@Id", new { rider.Id }))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        string[] required = [DocumentTypes.Aadhaar, DocumentTypes.DrivingLicence, DocumentTypes.VehicleRc, DocumentTypes.Selfie];
        if (required.Any(x => !types.Contains(x)) || string.IsNullOrWhiteSpace(rider.AccountNumber))
            throw AppException.Unprocessable("Aadhaar, DL, RC, selfie, and bank details are required.", "REGISTRATION_INCOMPLETE");

        await db.ExecuteAsync("UPDATE delRiders SET Status=@status, UpdatedAt=SYSUTCDATETIME() WHERE Id=@Id",
            new { rider.Id, status = RiderStatuses.Submitted });
    }

    public async Task SetPresenceAsync(Guid userId, PresenceRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rider = await RequireByUser(db, userId);
        if (rider.Status != RiderStatuses.Approved)
            throw AppException.Forbidden("Rider must be approved before going online.");

        await db.ExecuteAsync(
            "UPDATE delRiders SET Available=@Available, Latitude=@Latitude, Longitude=@Longitude, UpdatedAt=SYSUTCDATETIME() WHERE Id=@Id",
            new { rider.Id, request.Available, request.Latitude, request.Longitude });
    }

    public async Task<AssignmentDto?> GetCurrentAssignmentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rider = await RequireByUser(db, userId);
        var row = await db.QuerySingleOrDefaultAsync<RiderAssignment>(
            @"SELECT TOP 1 * FROM delAssignments
              WHERE RiderGuid=@Id AND Status='OFFERED' AND ExpiresAt > SYSUTCDATETIME()
              ORDER BY CreatedAt DESC", new { rider.Id });
        return row is null ? null : await MapAssignment(db, row, revealAddress: false);
    }

    public async Task<AssignmentDto> RespondAsync(Guid userId, string assignmentId, RespondAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rider = await RequireByUser(db, userId);
        var assignment = await db.QuerySingleOrDefaultAsync<RiderAssignment>(
            "SELECT * FROM delAssignments WHERE AssignmentId=@assignmentId AND RiderGuid=@Id",
            new { assignmentId, rider.Id }) ?? throw AppException.NotFound("Assignment not found.");

        if (assignment.ExpiresAt < DateTime.UtcNow)
            throw AppException.Unprocessable("Offer expired.", "NO_RIDER_AVAILABLE");

        var busy = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM delDeliveries WHERE RiderGuid=@Id AND Status IN ('ASSIGNED','ARRIVED_KITCHEN','PICKED_UP')",
            new { rider.Id });
        if (busy > 0 && request.Decision.Equals("ACCEPT", StringComparison.OrdinalIgnoreCase))
            throw AppException.Conflict("Rider is already on a trip.", "RIDER_BUSY");

        var decision = request.Decision.Equals("ACCEPT", StringComparison.OrdinalIgnoreCase) ? AssignmentStatuses.Accepted : AssignmentStatuses.Declined;
        await db.ExecuteAsync("UPDATE delAssignments SET Status=@decision WHERE Id=@Id", new { assignment.Id, decision });

        if (decision == AssignmentStatuses.Accepted)
        {
            await db.ExecuteAsync(
                @"INSERT INTO delDeliveries (Id, OrderGuid, RiderGuid, DeliveryMode, Status, CreatedAt, UpdatedAt)
                  VALUES (@Id, @OrderGuid, @RiderGuid, 'RIDER', 'ASSIGNED', SYSUTCDATETIME(), SYSUTCDATETIME())",
                new { Id = Guid.NewGuid(), assignment.OrderGuid, RiderGuid = rider.Id });
            return await MapAssignment(db, assignment, revealAddress: true);
        }

        var declines = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM delAssignments WHERE OrderGuid=@OrderGuid AND Status='DECLINED'",
            new { assignment.OrderGuid });
        if (declines >= _commerce.MaxRiderOffers)
            await FallbackKitchenSelf(db, assignment.OrderGuid);

        return await MapAssignment(db, assignment, revealAddress: false);
    }

    public async Task UpdateTripStatusAsync(Guid userId, string orderId, string status, CancellationToken cancellationToken = default)
    {
        if (status is not TripStatuses.ArrivedKitchen and not TripStatuses.PickedUp and not TripStatuses.Delivered)
            throw AppException.Validation("Trip status must be ARRIVED_KITCHEN, PICKED_UP, or DELIVERED.");

        using var db = _connections.Create();
        var rider = await RequireByUser(db, userId);
        var order = await OrderingService.RequireOrder(db, orderId);
        var job = await db.QuerySingleOrDefaultAsync<DeliveryJob>(
            "SELECT * FROM delDeliveries WHERE OrderGuid=@Id AND RiderGuid=@RiderId",
            new { order.Id, RiderId = rider.Id }) ?? throw AppException.NotFound("Trip not found.");

        await db.ExecuteAsync("UPDATE delDeliveries SET Status=@status, UpdatedAt=SYSUTCDATETIME() WHERE Id=@Id", new { job.Id, status });
        if (status == TripStatuses.PickedUp)
            await OrderingService.SetStatus(db, order.Id, OrderStatuses.OutForDelivery, "Rider picked up");
        if (status == TripStatuses.Delivered)
        {
            await OrderingService.SetStatus(db, order.Id, OrderStatuses.Delivered, "Delivered");
            await _settlements.CreditOnDeliveredAsync(order.Id, cancellationToken);
        }
    }

    public async Task UpdateTrackingAsync(string orderId, Guid userId, TrackingRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rider = await RequireByUser(db, userId);
        var order = await OrderingService.RequireOrder(db, orderId);
        await db.ExecuteAsync(
            "UPDATE delDeliveries SET Latitude=@Latitude, Longitude=@Longitude, UpdatedAt=SYSUTCDATETIME() WHERE OrderGuid=@Id AND RiderGuid=@RiderId",
            new { order.Id, RiderId = rider.Id, request.Latitude, request.Longitude });
    }

    public async Task ReviewAsync(string riderId, RiderReviewRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rider = await db.QuerySingleOrDefaultAsync<Rider>("SELECT * FROM delRiders WHERE RiderId=@riderId", new { riderId })
                    ?? throw AppException.NotFound("Rider not found.");
        var status = request.Decision.Equals("APPROVE", StringComparison.OrdinalIgnoreCase) ? RiderStatuses.Approved : RiderStatuses.Rejected;
        await db.ExecuteAsync("UPDATE delRiders SET Status=@status, UpdatedAt=SYSUTCDATETIME() WHERE Id=@Id", new { rider.Id, status });
    }

    public async Task StartAssignmentForReadyOrderAsync(Guid orderGuid, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await db.QuerySingleAsync<(decimal? Latitude, decimal? Longitude)>(
            @"SELECT k.Latitude, k.Longitude
              FROM ordOrders o INNER JOIN kitKitchens k ON k.Id=o.KitchenGuid
              WHERE o.Id=@orderGuid", new { orderGuid });

        if (kitchen.Latitude is null || kitchen.Longitude is null)
        {
            await FallbackKitchenSelf(db, orderGuid);
            return;
        }

        var riders = (await db.QueryAsync<Rider>(
            "SELECT * FROM delRiders WHERE Status='APPROVED' AND Available=1 AND Latitude IS NOT NULL")).ToList();

        var nearest = riders
            .Select(r => new { Rider = r, Distance = _geo.DistanceKm((double)kitchen.Latitude.Value, (double)kitchen.Longitude.Value, (double)r.Latitude!, (double)r.Longitude!) })
            .Where(x => x.Distance <= _commerce.RiderSearchKm)
            .OrderBy(x => x.Distance)
            .Take(_commerce.MaxRiderOffers)
            .ToList();

        if (nearest.Count == 0)
        {
            await FallbackKitchenSelf(db, orderGuid);
            return;
        }

        foreach (var item in nearest)
        {
            await db.ExecuteAsync(
                @"INSERT INTO delAssignments (Id, AssignmentId, OrderGuid, RiderGuid, Status, ExpiresAt, CreatedAt)
                  VALUES (@Id, @AssignmentId, @OrderGuid, @RiderGuid, 'OFFERED', DATEADD(SECOND, @ttl, SYSUTCDATETIME()), SYSUTCDATETIME())",
                new
                {
                    Id = Guid.NewGuid(),
                    AssignmentId = await _ids.NextAsync("ASG"),
                    OrderGuid = orderGuid,
                    RiderGuid = item.Rider.Id,
                    ttl = _commerce.RiderOfferSeconds
                });
        }
    }

    private async Task FallbackKitchenSelf(System.Data.IDbConnection db, Guid orderGuid)
    {
        await db.ExecuteAsync("UPDATE ordOrders SET DeliveryMode='KITCHEN_SELF', UpdatedAt=SYSUTCDATETIME() WHERE Id=@orderGuid", new { orderGuid });
        await db.ExecuteAsync(
            @"IF NOT EXISTS (SELECT 1 FROM delDeliveries WHERE OrderGuid=@orderGuid)
              INSERT INTO delDeliveries (Id, OrderGuid, DeliveryMode, Status, CreatedAt, UpdatedAt)
              VALUES (NEWID(), @orderGuid, 'KITCHEN_SELF', 'ASSIGNED', SYSUTCDATETIME(), SYSUTCDATETIME())",
            new { orderGuid });
        await OrderingService.SetStatus(db, orderGuid, OrderStatuses.OutForDelivery, "No rider available; kitchen self-delivery.");
    }

    private static async Task<Rider> RequireByUser(System.Data.IDbConnection db, Guid userId) =>
        await db.QuerySingleOrDefaultAsync<Rider>("SELECT * FROM delRiders WHERE UserId=@userId", new { userId })
        ?? throw AppException.NotFound("Rider profile not found.");

    private static async Task<AssignmentDto> MapAssignment(System.Data.IDbConnection db, RiderAssignment assignment, bool revealAddress)
    {
        var order = await db.QuerySingleAsync<(string OrderId, string DeliveryAddress)>(
            "SELECT OrderId, DeliveryAddress FROM ordOrders WHERE Id=@OrderGuid", new { assignment.OrderGuid });
        return new AssignmentDto
        {
            AssignmentId = assignment.AssignmentId,
            OrderId = order.OrderId,
            Status = assignment.Status,
            ExpiresAt = assignment.ExpiresAt,
            DropAddress = revealAddress ? order.DeliveryAddress : null
        };
    }
}
