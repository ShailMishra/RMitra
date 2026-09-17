using System.Data;
using System.Text.Json;
using Dapper;
using RMitra.Application.Abstractions;
using RMitra.Application.Kitchen;
using RMitra.Application.Subscription;
using RMitra.BuildingBlocks.Exceptions;
using RMitra.Domain.Common;
using KitchenEntity = RMitra.Domain.Kitchen.Kitchen;
using RMitra.Domain.Kitchen;
using RMitra.Infrastructure.Identity;

namespace RMitra.Infrastructure.Kitchen;

public class KitchenService : IKitchenService
{
    private readonly ISqlConnectionFactory _connections;
    private readonly IPublicIdGenerator _ids;
    private readonly IGeoCalculator _geo;
    private readonly ISubscriptionService _subscriptions;
    private readonly IdentityService _identity;

    public KitchenService(
        ISqlConnectionFactory connections,
        IPublicIdGenerator ids,
        IGeoCalculator geo,
        ISubscriptionService subscriptions,
        IdentityService identity)
    {
        _connections = connections;
        _ids = ids;
        _geo = geo;
        _subscriptions = subscriptions;
        _identity = identity;
    }

    public async Task<KitchenDto> CreateAsync(Guid ownerUserId, CreateKitchenRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(request.KitchenType, "HOMEMADE", StringComparison.OrdinalIgnoreCase))
            throw AppException.Validation("kitchenType must be HOMEMADE.");

        using var db = _connections.Create();
        var otp = await _identity.RequireFreshToken(db, request.VerificationToken);
        if (!string.Equals(otp.Purpose, "KITCHEN_OWNER", StringComparison.OrdinalIgnoreCase))
            throw AppException.Forbidden("OTP purpose must be KITCHEN_OWNER.");

        var existing = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM tblKitchens WHERE MobileNumber = @mobile",
            new { mobile = otp.MobileNumber });
        if (existing > 0)
            throw AppException.Conflict("A kitchen already exists for this mobile.", "KITCHEN_ALREADY_EXISTS");

        var owner = await EnsureOwner(db, otp.MobileNumber, request.OwnerName, request.Email);
        var kitchen = new KitchenEntity
        {
            Id = Guid.NewGuid(),
            KitchenId = await _ids.NextAsync(PublicIdPrefixes.Kitchen),
            OwnerUserId = owner.Id,
            KitchenName = request.KitchenName.Trim(),
            OwnerName = request.OwnerName.Trim(),
            MobileNumber = otp.MobileNumber,
            Email = request.Email.Trim(),
            KitchenType = "HOMEMADE",
            Status = KitchenStatuses.MobileVerified,
            DeliveryRadiusKm = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await db.ExecuteAsync(
            @"INSERT INTO tblKitchens
                (Id, KitchenId, OwnerUserId, KitchenName, OwnerName, MobileNumber, Email, KitchenType,
                 Status, ListingEnabled, DeliveryRadiusKm, CreatedAt, UpdatedAt)
              VALUES
                (@Id, @KitchenId, @OwnerUserId, @KitchenName, @OwnerName, @MobileNumber, @Email, @KitchenType,
                 @Status, 0, @DeliveryRadiusKm, @CreatedAt, @UpdatedAt)",
            kitchen);

        await History(db, kitchen.Id, kitchen.Status, "Draft created");
        return Map(kitchen);
    }

    public async Task<KitchenDto> PatchAsync(string kitchenId, Guid ownerUserId, PatchKitchenRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await RequireOwned(db, kitchenId, ownerUserId);

        if (request.CuisineTypes is { Count: > 0 })
        {
            if (request.CuisineTypes.Count is < 1 or > 8)
                throw AppException.Validation("Select 1 to 8 cuisine codes.");
        }

        kitchen.KitchenName = request.KitchenName ?? kitchen.KitchenName;
        kitchen.OwnerName = request.OwnerName ?? kitchen.OwnerName;
        kitchen.Email = request.Email ?? kitchen.Email;
        kitchen.AddressLine1 = request.AddressLine1 ?? kitchen.AddressLine1;
        kitchen.AddressLine2 = request.AddressLine2 ?? kitchen.AddressLine2;
        kitchen.City = request.City ?? kitchen.City;
        kitchen.State = request.State ?? kitchen.State;
        kitchen.Pincode = request.Pincode ?? kitchen.Pincode;
        kitchen.Latitude = request.Latitude ?? kitchen.Latitude;
        kitchen.Longitude = request.Longitude ?? kitchen.Longitude;
        kitchen.CuisineTypes = request.CuisineTypes is null ? kitchen.CuisineTypes : JsonSerializer.Serialize(request.CuisineTypes);
        kitchen.OpenTime = request.OpenTime ?? kitchen.OpenTime;
        kitchen.CloseTime = request.CloseTime ?? kitchen.CloseTime;
        kitchen.DeliveryRadiusKm = request.DeliveryRadiusKm ?? kitchen.DeliveryRadiusKm;
        if (request.BankDetails is not null)
        {
            kitchen.AccountHolderName = request.BankDetails.Holder;
            kitchen.AccountNumber = request.BankDetails.Account;
            kitchen.IfscCode = request.BankDetails.Ifsc;
            kitchen.AccountType = request.BankDetails.Type;
        }

        await db.ExecuteAsync(
            @"UPDATE tblKitchens SET
                KitchenName=@KitchenName, OwnerName=@OwnerName, Email=@Email,
                AddressLine1=@AddressLine1, AddressLine2=@AddressLine2, City=@City, State=@State, Pincode=@Pincode,
                Latitude=@Latitude, Longitude=@Longitude, CuisineTypes=@CuisineTypes, OpenTime=@OpenTime, CloseTime=@CloseTime,
                DeliveryRadiusKm=@DeliveryRadiusKm, AccountHolderName=@AccountHolderName, AccountNumber=@AccountNumber,
                IfscCode=@IfscCode, AccountType=@AccountType, UpdatedAt=SYSUTCDATETIME()
              WHERE KitchenId=@KitchenId",
            kitchen);

        return Map(kitchen);
    }

    public async Task<KitchenDto> GetAsync(string kitchenId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        return Map(await Require(db, kitchenId));
    }

    public async Task<KitchenDocumentsResponse> AddDocumentAsync(string kitchenId, Guid ownerUserId, string documentType, string fileUrl, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await RequireOwned(db, kitchenId, ownerUserId);
        await db.ExecuteAsync(
            @"INSERT INTO tblKitchenDocuments (Id, KitchenGuid, DocumentType, FileUrl, CreatedAt)
              VALUES (@Id, @KitchenGuid, @DocumentType, @FileUrl, SYSUTCDATETIME())",
            new { Id = Guid.NewGuid(), KitchenGuid = kitchen.Id, DocumentType = documentType, FileUrl = fileUrl });
        return await LoadDocuments(db, kitchen.Id);
    }

    public async Task<KitchenDocumentsResponse> GetDocumentsAsync(string kitchenId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await Require(db, kitchenId);
        return await LoadDocuments(db, kitchen.Id);
    }

    public async Task<KitchenDto> SubmitAsync(string kitchenId, Guid ownerUserId, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await RequireOwned(db, kitchenId, ownerUserId);

        if (string.IsNullOrWhiteSpace(kitchen.KitchenName) || string.IsNullOrWhiteSpace(kitchen.AddressLine1) ||
            string.IsNullOrWhiteSpace(kitchen.CuisineTypes) || string.IsNullOrWhiteSpace(kitchen.AccountNumber))
            throw AppException.Unprocessable("Complete all four registration steps before submit.", "REGISTRATION_INCOMPLETE");

        var docs = await LoadDocuments(db, kitchen.Id);
        if (docs.MissingRequired.Count > 0)
            throw AppException.Unprocessable("KITCHEN_PHOTO and PAN_CARD are required. FSSAI is optional.", "REGISTRATION_INCOMPLETE");

        var next = kitchen.Status is KitchenStatuses.AdditionalDocumentsRequired or KitchenStatuses.Rejected
            ? KitchenStatuses.Resubmitted
            : KitchenStatuses.Submitted;

        await db.ExecuteAsync(
            @"UPDATE tblKitchens
              SET Status=@status, SubmittedAt=SYSUTCDATETIME(), UpdatedAt=SYSUTCDATETIME(),
                  RejectionReason=NULL, AdditionalDocumentsRequired=NULL
              WHERE KitchenId=@kitchenId",
            new { kitchenId, status = next });

        await History(db, kitchen.Id, next, "Submitted for review");

        if (next == KitchenStatuses.Submitted)
        {
            await db.ExecuteAsync(
                "UPDATE tblKitchens SET Status=@pending WHERE KitchenId=@kitchenId",
                new { kitchenId, pending = KitchenStatuses.DocumentVerificationPending });
            await History(db, kitchen.Id, KitchenStatuses.DocumentVerificationPending, "Queued for document checks");
        }

        return Map(await Require(db, kitchenId));
    }

    public async Task<KitchenProgressDto> GetProgressAsync(string kitchenId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await Require(db, kitchenId);
        var status = kitchen.Status;
        return new KitchenProgressDto
        {
            KitchenId = kitchen.KitchenId,
            Status = status,
            Checklist =
            [
                new() { Key = "MOBILE_VERIFIED", Label = "Mobile Verified", Done = true },
                new() { Key = "SUBMITTED", Label = "Submitted", Done = IsAtLeast(status, KitchenStatuses.Submitted) },
                new() { Key = "DOCUMENTS", Label = "Documents Under Review", Done = IsAtLeast(status, KitchenStatuses.UnderReview) || status is KitchenStatuses.Approved or KitchenStatuses.MenuSetup or KitchenStatuses.Active },
                new() { Key = "APPROVAL", Label = "Approval Pending", Done = status is KitchenStatuses.Approved or KitchenStatuses.MenuSetup or KitchenStatuses.Active },
                new() { Key = "ACTIVATE", Label = "Activate", Done = status == KitchenStatuses.Active }
            ]
        };
    }

    public async Task<KitchenDto> ActivateAsync(string kitchenId, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await RequireOwned(db, kitchenId, ownerUserId);
        if (kitchen.Status is not KitchenStatuses.Approved and not KitchenStatuses.MenuSetup)
            throw AppException.Unprocessable("Kitchen must be approved before activate.", "REGISTRATION_INCOMPLETE");

        var published = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM tblMenuItems WHERE KitchenGuid=@Id AND Status='PUBLISHED'",
            new { kitchen.Id });
        if (published == 0)
            throw AppException.Unprocessable("Publish at least one menu item before activate.", "MENU_REQUIRED");

        await db.ExecuteAsync(
            "UPDATE tblKitchens SET Status=@status, ActivatedAt=SYSUTCDATETIME(), UpdatedAt=SYSUTCDATETIME() WHERE KitchenId=@kitchenId",
            new { kitchenId, status = KitchenStatuses.Active });
        await History(db, kitchen.Id, KitchenStatuses.Active, "Kitchen activated");
        return Map(await Require(db, kitchenId));
    }

    public async Task<KitchenDashboardDto> GetDashboardAsync(string kitchenId, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await RequireOwned(db, kitchenId, ownerUserId);
        var stats = await db.QuerySingleAsync<(int TodayOrders, decimal TodayEarnings)>(
            @"SELECT
                COUNT(1) AS TodayOrders,
                ISNULL(SUM(CASE WHEN Status='DELIVERED' THEN ItemTotal ELSE 0 END),0) AS TodayEarnings
              FROM tblOrders
              WHERE KitchenGuid=@Id AND CAST(CreatedAt AS DATE) = CAST(SYSUTCDATETIME() AS DATE)",
            new { kitchen.Id });

        return new KitchenDashboardDto
        {
            KitchenId = kitchen.KitchenId,
            Status = kitchen.Status,
            TodayOrders = stats.TodayOrders,
            TodayEarnings = stats.TodayEarnings
        };
    }

    public async Task<IReadOnlyList<KitchenDto>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rows = await db.QueryAsync<KitchenEntity>(
            "uspGetPendingKitchens",
            commandType: CommandType.StoredProcedure);
        return rows.Select(Map).ToList();
    }

    public async Task<KitchenDto> ApproveAsync(string kitchenId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await Require(db, kitchenId);
        await db.ExecuteAsync(
            @"UPDATE tblKitchens
              SET Status=@status, ListingEnabled=1, ApprovedAt=SYSUTCDATETIME(), UpdatedAt=SYSUTCDATETIME()
              WHERE KitchenId=@kitchenId",
            new { kitchenId, status = KitchenStatuses.Approved });
        await History(db, kitchen.Id, KitchenStatuses.Approved, "Approved");
        return Map(await Require(db, kitchenId));
    }

    public async Task<KitchenDto> RejectAsync(string kitchenId, RejectKitchenRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await Require(db, kitchenId);
        await db.ExecuteAsync(
            "UPDATE tblKitchens SET Status=@status, RejectionReason=@Reason, ListingEnabled=0, UpdatedAt=SYSUTCDATETIME() WHERE KitchenId=@kitchenId",
            new { kitchenId, status = KitchenStatuses.Rejected, request.Reason });
        await History(db, kitchen.Id, KitchenStatuses.Rejected, request.Reason);
        return Map(await Require(db, kitchenId));
    }

    public async Task<KitchenDto> RequestDocumentsAsync(string kitchenId, RequestDocumentsRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchen = await Require(db, kitchenId);
        await db.ExecuteAsync(
            "UPDATE tblKitchens SET Status=@status, AdditionalDocumentsRequired=@docs, UpdatedAt=SYSUTCDATETIME() WHERE KitchenId=@kitchenId",
            new { kitchenId, status = KitchenStatuses.AdditionalDocumentsRequired, docs = request.DocumentsRequired });
        await History(db, kitchen.Id, KitchenStatuses.AdditionalDocumentsRequired, request.DocumentsRequired);
        return Map(await Require(db, kitchenId));
    }

    public async Task<IReadOnlyList<NearbyKitchenDto>> NearbyAsync(decimal latitude, decimal longitude, bool openNow, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var kitchens = (await db.QueryAsync<KitchenEntity>(
            "uspGetNearby",
            commandType: CommandType.StoredProcedure)).ToList();

        var result = new List<NearbyKitchenDto>();
        foreach (var kitchen in kitchens)
        {
            var distance = _geo.DistanceKm((double)latitude, (double)longitude, (double)kitchen.Latitude!, (double)kitchen.Longitude!);
            if (distance > kitchen.DeliveryRadiusKm)
                continue;

            var isOpen = IsOpenNow(kitchen.OpenTime, kitchen.CloseTime);
            if (openNow && !isOpen)
                continue;

            result.Add(new NearbyKitchenDto
            {
                KitchenId = kitchen.KitchenId,
                KitchenName = kitchen.KitchenName,
                CuisineTypes = Deserialize(kitchen.CuisineTypes),
                DistanceKm = distance,
                OpenNow = isOpen,
                Featured = await _subscriptions.IsFeaturedKitchenAsync(kitchen.OwnerUserId, cancellationToken),
                OpenTime = kitchen.OpenTime,
                CloseTime = kitchen.CloseTime
            });
        }

        return result.OrderByDescending(x => x.Featured).ThenBy(x => x.DistanceKm).ToList();
    }

    private async Task<Domain.Identity.User> EnsureOwner(System.Data.IDbConnection db, string mobile, string name, string email)
    {
        var user = await IdentityService.FindUser(db, mobile, "KITCHEN_OWNER");
        return user ?? await _identity.InsertUser(db, mobile, "KITCHEN_OWNER", name, null, email);
    }

    internal static async Task<KitchenEntity> Require(System.Data.IDbConnection db, string kitchenId) =>
        await db.QuerySingleOrDefaultAsync<KitchenEntity>(
            "uspGetKitchen",
            new { kitchenId },
            commandType: CommandType.StoredProcedure)
        ?? throw AppException.NotFound("Kitchen not found.");

    internal static async Task<KitchenEntity> RequireOwned(System.Data.IDbConnection db, string kitchenId, Guid ownerUserId)
    {
        var kitchen = await Require(db, kitchenId);
        if (kitchen.OwnerUserId != ownerUserId)
            throw AppException.Forbidden();
        return kitchen;
    }

    internal static async Task EnsureListingEnabled(System.Data.IDbConnection db, Guid kitchenGuid)
    {
        var enabled = await db.ExecuteScalarAsync<bool>(
            "SELECT ListingEnabled FROM tblKitchens WHERE Id=@kitchenGuid", new { kitchenGuid });
        if (!enabled)
            throw AppException.ListingNotEnabled();
    }

    private static async Task<KitchenDocumentsResponse> LoadDocuments(System.Data.IDbConnection db, Guid kitchenGuid)
    {
        var rows = (await db.QueryAsync<KitchenDocument>(
            "SELECT * FROM tblKitchenDocuments WHERE KitchenGuid=@kitchenGuid", new { kitchenGuid })).ToList();

        var types = rows.Select(x => x.DocumentType).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = new List<string>();
        if (!types.Contains(DocumentTypes.KitchenPhoto)) missing.Add(DocumentTypes.KitchenPhoto);
        if (!types.Contains(DocumentTypes.PanCard)) missing.Add(DocumentTypes.PanCard);

        return new KitchenDocumentsResponse
        {
            Documents = rows.Select(x => new KitchenDocumentDto
            {
                DocumentType = x.DocumentType,
                FileUrl = x.FileUrl,
                Required = x.DocumentType is DocumentTypes.KitchenPhoto or DocumentTypes.PanCard
            }).ToList(),
            MissingRequired = missing
        };
    }

    private static async Task History(System.Data.IDbConnection db, Guid kitchenGuid, string status, string remarks) =>
        await db.ExecuteAsync(
            "INSERT INTO tblKitchenStatusHistory (KitchenGuid, Status, Remarks, CreatedAt) VALUES (@kitchenGuid, @status, @remarks, SYSUTCDATETIME())",
            new { kitchenGuid, status, remarks });

    private static bool IsAtLeast(string status, string target)
    {
        string[] order =
        [
            KitchenStatuses.Draft, KitchenStatuses.MobileVerified, KitchenStatuses.Submitted,
            KitchenStatuses.DocumentVerificationPending, KitchenStatuses.UnderReview,
            KitchenStatuses.AdditionalDocumentsRequired, KitchenStatuses.Resubmitted,
            KitchenStatuses.Approved, KitchenStatuses.MenuSetup, KitchenStatuses.Active
        ];
        return Array.IndexOf(order, status) >= Array.IndexOf(order, target);
    }

    private static bool IsOpenNow(string? open, string? close)
    {
        if (!TimeSpan.TryParse(open, out var start) || !TimeSpan.TryParse(close, out var end))
            return true;
        var now = DateTime.UtcNow.TimeOfDay;
        return start <= end ? now >= start && now <= end : now >= start || now <= end;
    }

    internal static KitchenDto Map(KitchenEntity kitchen) => new()
    {
        KitchenId = kitchen.KitchenId,
        KitchenName = kitchen.KitchenName,
        OwnerName = kitchen.OwnerName,
        MobileNumber = kitchen.MobileNumber,
        Email = kitchen.Email,
        KitchenType = kitchen.KitchenType,
        Status = kitchen.Status,
        ListingEnabled = kitchen.ListingEnabled,
        AddressLine1 = kitchen.AddressLine1,
        AddressLine2 = kitchen.AddressLine2,
        City = kitchen.City,
        State = kitchen.State,
        Pincode = kitchen.Pincode,
        Latitude = kitchen.Latitude,
        Longitude = kitchen.Longitude,
        CuisineTypes = Deserialize(kitchen.CuisineTypes),
        OpenTime = kitchen.OpenTime,
        CloseTime = kitchen.CloseTime,
        DeliveryRadiusKm = kitchen.DeliveryRadiusKm,
        RejectionReason = kitchen.RejectionReason
    };

    private static List<string> Deserialize(string? json)
    {
        try { return string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }
}
