using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMitra.Api.ExtensionMethod;
using RMitra.Application.Delivery;
using RMitra.Application.Ordering;
using RMitra.Application.Payment;
using RMitra.BuildingBlocks.Security;

namespace RMitra.Api.Controllers;

[Authorize]
[Route("/api/homely/order")]
[Tags("Ordering")]
public class OrderingController : ApiControllerBase
{
    private readonly IOrderingService _orders;
    private readonly IPaymentService _payments;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<OrderingController> _logger;

    public OrderingController(IOrderingService orders, IPaymentService payments, ICurrentUser currentUser, ILogger<OrderingController> logger)
    {
        _orders = orders;
        _payments = payments;
        _currentUser = currentUser;
        _logger = logger;
    }

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [Route("get_cart")]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken) =>
        this.OkCustom(null, await _orders.GetCartAsync(_currentUser.UserId, cancellationToken), _logger);

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [Route("update_cart")]
    public async Task<IActionResult> UpdateCart([FromBody] PutCartRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _orders.PutCartAsync(_currentUser.UserId, request, cancellationToken), _logger);
    }

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [Route("place")]
    public async Task<IActionResult> Place([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _orders.PlaceOrderAsync(_currentUser.UserId, request, IdempotencyKey, cancellationToken), _logger);
    }

    [HttpPost]
    [Route("get_list")]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken) =>
        this.OkCustom(null, await _orders.GetMyOrdersAsync(_currentUser.UserId, _currentUser.Role, cancellationToken), _logger);

    [HttpPost]
    [Route("get/{orderId}")]
    public async Task<IActionResult> Get(string orderId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _orders.GetOrderAsync(orderId, _currentUser.UserId, _currentUser.Role, cancellationToken), _logger);

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [Route("cancel/{orderId}")]
    public async Task<IActionResult> Cancel(string orderId, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _orders.CancelAsync(orderId, _currentUser.UserId, request, cancellationToken), _logger);
    }

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [Route("create_payment/{orderId}")]
    public async Task<IActionResult> CreatePayment(string orderId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _payments.CreateForOrderAsync(orderId, _currentUser.UserId, cancellationToken), _logger);

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [Route("rate/{orderId}")]
    public async Task<IActionResult> Rate(string orderId, [FromBody] RatingRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        await _orders.RateAsync(orderId, _currentUser.UserId, request, cancellationToken);
        return this.OkCustom(request, new { orderId, request.Rating }, _logger);
    }

    [Authorize(Roles = Roles.Rider)]
    [HttpPost]
    [Route("update_tracking/{orderId}")]
    public async Task<IActionResult> UpdateTracking(string orderId, [FromBody] TrackingRequest request, [FromServices] IRiderService riders, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        await riders.UpdateTrackingAsync(orderId, _currentUser.UserId, request, cancellationToken);
        return this.OkCustom(request, new { orderId, request.Latitude, request.Longitude }, _logger);
    }
}
