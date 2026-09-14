namespace RMitra.Domain.Subscription;

public class SubscriptionPlan
{
    public string PlanCode { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
}

public class Subscription
{
    public Guid Id { get; set; }
    public string SubscriptionId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public DateTime ValidUntil { get; set; }
    public bool AutoRenew { get; set; }
    public DateTime CreatedAt { get; set; }
}
