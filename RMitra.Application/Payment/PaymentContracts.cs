using System.ComponentModel.DataAnnotations;

namespace RMitra.Application.Payment;

public class PaymentDto
{
    public string PaymentId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? GatewayReference { get; set; }
}

public class ConfirmPaymentRequest
{
    public bool Success { get; set; } = true;
    public string? Signature { get; set; }
    public string? GatewayReference { get; set; }
    public string? FailureReason { get; set; }
}

public class PaymentWebhookRequest
{
    [Required] public string PaymentId { get; set; } = string.Empty;
    [Required] public string Status { get; set; } = string.Empty;
    public string? GatewayReference { get; set; }
}

public interface IPaymentService
{
    Task<PaymentDto> CreateForOrderAsync(string orderId, Guid userId, CancellationToken cancellationToken = default);
    Task<PaymentDto> ConfirmAsync(string paymentId, ConfirmPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentDto> ProcessWebhookAsync(PaymentWebhookRequest request, CancellationToken cancellationToken = default);
}
