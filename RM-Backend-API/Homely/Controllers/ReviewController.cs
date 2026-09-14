using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMitra.Api.ExtensionMethod;
using RMitra.Application.Catalog;
using RMitra.Application.Settlement;
using RMitra.Application.Subscription;
using RMitra.BuildingBlocks.Security;

namespace RMitra.Api.Controllers;

[Route("/api/homely/subscription")]
[Tags("Subscriptions")]
public class SubscriptionController : ApiControllerBase
{
    private readonly ISubscriptionService _subscriptions;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(ISubscriptionService subscriptions, ICurrentUser currentUser, ILogger<SubscriptionController> logger)
    {
        _subscriptions = subscriptions;
        _currentUser = currentUser;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("get_plans")]
    public async Task<IActionResult> GetPlans([FromQuery] string? audience, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _subscriptions.GetPlansAsync(audience, cancellationToken), _logger);

    [Authorize]
    [HttpPost]
    [Route("purchase")]
    public async Task<IActionResult> Purchase([FromBody] PurchaseSubscriptionRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _subscriptions.PurchaseAsync(_currentUser.UserId, _currentUser.Role, request, cancellationToken), _logger);
    }

    [Authorize]
    [HttpPost]
    [Route("get_me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken) =>
        this.OkCustom(null, await _subscriptions.GetMineAsync(_currentUser.UserId, cancellationToken), _logger);

    [Authorize]
    [HttpPost]
    [Route("cancel/{id}")]
    public async Task<IActionResult> Cancel(string id, CancellationToken cancellationToken)
    {
        await _subscriptions.CancelAsync(id, _currentUser.UserId, cancellationToken);
        return this.OkCustom(null, new { id, cancelled = true }, _logger);
    }
}

[Route("/api/homely/settlement")]
[Tags("Settlement")]
public class SettlementController : ApiControllerBase
{
    private readonly ISettlementService _settlements;
    private readonly ILogger<SettlementController> _logger;

    public SettlementController(ISettlementService settlements, ILogger<SettlementController> logger)
    {
        _settlements = settlements;
        _logger = logger;
    }

    [Authorize]
    [HttpPost]
    [Route("get_list")]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken) =>
        this.OkCustom(null, await _settlements.ListAsync(cancellationToken), _logger);

    [Authorize]
    [HttpPost]
    [Route("get/{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _settlements.GetAsync(id, cancellationToken), _logger);
}

[Route("/api/homely/master")]
[Tags("Master")]
public class MasterController : ApiControllerBase
{
    private readonly ICatalogService _catalog;
    private readonly ILogger<MasterController> _logger;

    public MasterController(ICatalogService catalog, ILogger<MasterController> logger)
    {
        _catalog = catalog;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("get_cuisines")]
    public async Task<IActionResult> GetCuisines(CancellationToken cancellationToken) =>
        this.OkCustom(null, await _catalog.GetCuisinesAsync(cancellationToken), _logger);
}
