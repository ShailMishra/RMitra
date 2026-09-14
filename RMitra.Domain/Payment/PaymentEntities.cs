namespace RMitra.Domain.Payment;

public class Payment
{
    public Guid Id { get; set; }
    public string PaymentId { get; set; } = string.Empty;
    public Guid OrderGuid { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? GatewayReference { get; set; }
    public string? FailureReason { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
