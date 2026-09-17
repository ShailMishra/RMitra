using System.Data;
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
            "uspGetPlans",
            new { audience },
            commandType: CommandType.StoredProcedure);
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
            "uspGetPlan",
            request,
            commandType: CommandType.StoredProcedure)
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
        await db.ExecuteAsync("uspPurchase", sub, commandType: CommandType.StoredProcedure);
        return Map(sub);
    }

    public async Task<SubscriptionDto?> GetMineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var sub = await db.QuerySingleOrDefaultAsync<SubscriptionEntity>(
            "uspGetSubscriptionMe",
            new { userId },
            commandType: CommandType.StoredProcedure);
        return sub is null ? null : Map(sub);
    }

    public async Task CancelAsync(string subscriptionId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        var updated = await db.ExecuteScalarAsync<int>(
            "uspCancelSubscription",
            new { subscriptionId, userId },
            commandType: CommandType.StoredProcedure);
        if (updated == 0)
            throw AppException.NotFound("Subscription not found.");
    }

    public async Task<bool> HasActiveHomelyPlusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        return await db.ExecuteScalarAsync<int>(
            "uspHasActiveHomelyPlus",
            new { userId },
            commandType: CommandType.StoredProcedure) > 0;
    }

    public async Task<bool> IsFeaturedKitchenAsync(Guid kitchenOwnerUserId, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        return await db.ExecuteScalarAsync<int>(
            "uspIsFeaturedKitchen",
            new { kitchenOwnerUserId },
            commandType: CommandType.StoredProcedure) > 0;
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
