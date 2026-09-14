namespace RMitra.Application.Subscription;

public class PlanDto
{
    public string PlanCode { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class PurchaseSubscriptionRequest
{
    public string PlanCode { get; set; } = string.Empty;
}

public class SubscriptionDto
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string PlanCode { get; set; } = string.Empty;
    public DateTime ValidUntil { get; set; }
    public bool AutoRenew { get; set; }
    public IReadOnlyList<string> Benefits { get; set; } = [];
}

public interface ISubscriptionService
{
    Task<IReadOnlyList<PlanDto>> GetPlansAsync(string? audience, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> PurchaseAsync(Guid userId, string role, PurchaseSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<SubscriptionDto?> GetMineAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CancelAsync(string subscriptionId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveHomelyPlusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsFeaturedKitchenAsync(Guid kitchenOwnerUserId, CancellationToken cancellationToken = default);
}
