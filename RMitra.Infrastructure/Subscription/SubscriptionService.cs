using Dapper;
using RMitra.Application.Abstractions;
using RMitra.Application.Subscription;
using RMitra.BuildingBlocks.Exceptions;
using RMitra.BuildingBlocks.Security;
using RMitra.Domain.Common;
using SubscriptionEntity = RMitra.Domain.Subscription.Subscription;
using RMitra.Domain.Subscription;

namespace RMitra.Infrastructure.Subscription;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISqlConnectionFactory _connections;
    private readonly IPublicIdGenerator _ids;

    public SubscriptionService(ISqlConnectionFactory connections, IPublicIdGenerator ids)
    {
        _connections = connections;
        _ids = ids;
    }

    public async Task<IReadOnlyList<PlanDto>> GetPlansAsync(string? audience, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var rows = await db.QueryAsync<SubscriptionPlan>(
            "SELECT * FROM mstPlans WHERE @audience IS NULL OR Audience=@audience ORDER BY Price",
            new { audience });
        return rows.Select(x => new PlanDto
        {
            PlanCode = x.PlanCode,
            Audience = x.Audience,
            Name = x.Name,
            Price = x.Price
        }).ToList();
    }

    public async Task<SubscriptionDto> PurchaseAsync(Guid userId, string role, PurchaseSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var plan = await db.QuerySingleOrDefaultAsync<SubscriptionPlan>(
            "SELECT * FROM mstPlans WHERE PlanCode=@PlanCode", request)
            ?? throw AppException.NotFound("Plan not found.");

        if (plan.Audience == "CUSTOMER" && role != Roles.Customer)
            throw AppException.Forbidden();
        if (plan.Audience == "KITCHEN" && role != Roles.KitchenOwner)
            throw AppException.Forbidden();

        var sub = new SubscriptionEntity
        {
            Id = Guid.NewGuid(),
            SubscriptionId = await _ids.NextAsync(PublicIdPrefixes.Subscription),
            UserId = userId,
            PlanCode = plan.PlanCode,
            Audience = plan.Audience,
            ValidUntil = DateTime.UtcNow.AddDays(plan.DurationDays),
            AutoRenew = true,
            CreatedAt = DateTime.UtcNow
        };
        await db.ExecuteAsync(
            @"INSERT INTO subSubscriptions (Id, SubscriptionId, UserId, PlanCode, Audience, ValidUntil, AutoRenew, CreatedAt)
              VALUES (@Id, @SubscriptionId, @UserId, @PlanCode, @Audience, @ValidUntil, @AutoRenew, @CreatedAt)",
            sub);
        return Map(sub);
    }

    public async Task<SubscriptionDto?> GetMineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var sub = await db.QuerySingleOrDefaultAsync<SubscriptionEntity>(
            "SELECT TOP 1 * FROM subSubscriptions WHERE UserId=@userId AND ValidUntil>SYSUTCDATETIME() ORDER BY ValidUntil DESC",
            new { userId });
        return sub is null ? null : Map(sub);
    }

    public async Task CancelAsync(string subscriptionId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var updated = await db.ExecuteAsync(
            "UPDATE subSubscriptions SET AutoRenew=0 WHERE SubscriptionId=@subscriptionId AND UserId=@userId",
            new { subscriptionId, userId });
        if (updated == 0)
            throw AppException.NotFound("Subscription not found.");
    }

    public async Task<bool> HasActiveHomelyPlusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        return await db.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM subSubscriptions
              WHERE UserId=@userId AND Audience='CUSTOMER' AND ValidUntil>SYSUTCDATETIME()",
            new { userId }) > 0;
    }

    public async Task<bool> IsFeaturedKitchenAsync(Guid kitchenOwnerUserId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        return await db.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM subSubscriptions
              WHERE UserId=@kitchenOwnerUserId AND Audience='KITCHEN' AND ValidUntil>SYSUTCDATETIME()",
            new { kitchenOwnerUserId }) > 0;
    }

    private static SubscriptionDto Map(SubscriptionEntity sub) => new()
    {
        SubscriptionId = sub.SubscriptionId,
        PlanCode = sub.PlanCode,
        ValidUntil = sub.ValidUntil,
        AutoRenew = sub.AutoRenew,
        Benefits = sub.Audience == "CUSTOMER"
            ? ["Free delivery when itemTotal >= 199", "5% off items"]
            : ["Higher rank in nearby"]
    };
}
