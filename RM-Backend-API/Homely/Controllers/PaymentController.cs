using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMitra.Api.ExtensionMethod;
using RMitra.Application.Payment;

namespace RMitra.Api.Controllers;

[Route("/api/homely/payment")]
[Tags("Payment")]
public class PaymentController : ApiControllerBase
{
    private readonly IPaymentService _payments;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService payments, ILogger<PaymentController> logger)
    {
        _payments = payments;
        _logger = logger;
    }

    [Authorize]
    [HttpPost]
    [Route("confirm/{paymentId}")]
    public async Task<IActionResult> Confirm(string paymentId, [FromBody] ConfirmPaymentRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _payments.ConfirmAsync(paymentId, request, cancellationToken), _logger);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("webhook")]
    public async Task<IActionResult> Webhook([FromBody] PaymentWebhookRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _payments.ProcessWebhookAsync(request, cancellationToken), _logger);
    }
}
