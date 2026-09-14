using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMitra.Api.ExtensionMethod;
using RMitra.Api.Uploads;
using RMitra.Application.Catalog;
using RMitra.Application.Kitchen;
using RMitra.Application.Ordering;
using RMitra.Application.Settlement;
using RMitra.BuildingBlocks.Security;

namespace RMitra.Api.Controllers;

[Route("/api/homely/kitchen")]
[Tags("Kitchen")]
public class KitchenController : ApiControllerBase
{
    private readonly IKitchenService _kitchens;
    private readonly ICatalogService _catalog;
    private readonly IOrderingService _orders;
    private readonly ISettlementService _settlements;
    private readonly ICurrentUser _currentUser;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<KitchenController> _logger;

    public KitchenController(
        IKitchenService kitchens,
        ICatalogService catalog,
        IOrderingService orders,
        ISettlementService settlements,
        ICurrentUser currentUser,
        IWebHostEnvironment env,
        ILogger<KitchenController> logger)
    {
        _kitchens = kitchens;
        _catalog = catalog;
        _orders = orders;
        _settlements = settlements;
        _currentUser = currentUser;
        _env = env;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] CreateKitchenRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _kitchens.CreateAsync(
            _currentUser.IsAuthenticated ? _currentUser.UserId : Guid.Empty, request, cancellationToken), _logger);
    }

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("update/{kitchenId}")]
    public async Task<IActionResult> Update(string kitchenId, [FromBody] PatchKitchenRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _kitchens.PatchAsync(kitchenId, _currentUser.UserId, request, cancellationToken), _logger);
    }

    [Authorize]
    [HttpPost]
    [Route("get/{kitchenId}")]
    public async Task<IActionResult> Get(string kitchenId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _kitchens.GetAsync(kitchenId, cancellationToken), _logger);

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("upload_document/{kitchenId}")]
    public async Task<IActionResult> UploadDocument(string kitchenId, [FromForm] string documentType, IFormFile file, CancellationToken cancellationToken)
    {
        var url = await LocalFileStore.SaveAsync(_env, file, cancellationToken);
        return this.OkCustom(null, await _kitchens.AddDocumentAsync(kitchenId, _currentUser.UserId, documentType, url, cancellationToken), _logger);
    }

    [Authorize]
    [HttpPost]
    [Route("get_documents/{kitchenId}")]
    public async Task<IActionResult> GetDocuments(string kitchenId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _kitchens.GetDocumentsAsync(kitchenId, cancellationToken), _logger);

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("submit/{kitchenId}")]
    public async Task<IActionResult> Submit(string kitchenId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _kitchens.SubmitAsync(kitchenId, _currentUser.UserId, IdempotencyKey, cancellationToken), _logger);

    [Authorize]
    [HttpPost]
    [Route("get_progress/{kitchenId}")]
    public async Task<IActionResult> GetProgress(string kitchenId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _kitchens.GetProgressAsync(kitchenId, cancellationToken), _logger);

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("activate/{kitchenId}")]
    public async Task<IActionResult> Activate(string kitchenId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _kitchens.ActivateAsync(kitchenId, _currentUser.UserId, cancellationToken), _logger);

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("get_dashboard/{kitchenId}")]
    public async Task<IActionResult> GetDashboard(string kitchenId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _kitchens.GetDashboardAsync(kitchenId, _currentUser.UserId, cancellationToken), _logger);

    [AllowAnonymous]
    [HttpPost]
    [Route("get_nearby")]
    public async Task<IActionResult> GetNearby([FromQuery] decimal latitude, [FromQuery] decimal longitude, [FromQuery] bool openNow = false, CancellationToken cancellationToken = default) =>
        this.OkCustom(null, await _kitchens.NearbyAsync(latitude, longitude, openNow, cancellationToken), _logger);

    [AllowAnonymous]
    [HttpPost]
    [Route("get_storefront/{kitchenId}")]
    public async Task<IActionResult> GetStorefront(string kitchenId, [FromQuery] decimal? latitude, [FromQuery] decimal? longitude, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _catalog.GetStorefrontAsync(kitchenId, latitude, longitude, cancellationToken), _logger);

    [Authorize]
    [HttpPost]
    [Route("get_menu_categories/{kitchenId}")]
    public async Task<IActionResult> GetMenuCategories(string kitchenId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _catalog.GetCategoriesAsync(kitchenId, cancellationToken), _logger);

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("add_menu_category/{kitchenId}")]
    public async Task<IActionResult> AddMenuCategory(string kitchenId, [FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _catalog.AddCategoryAsync(kitchenId, _currentUser.UserId, request, cancellationToken), _logger);
    }

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("add_menu_item/{kitchenId}")]
    public async Task<IActionResult> AddMenuItem(string kitchenId, [FromBody] CreateMenuItemRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _catalog.AddItemAsync(kitchenId, _currentUser.UserId, request, cancellationToken), _logger);
    }

    [Authorize]
    [HttpPost]
    [Route("get_menu_items/{kitchenId}")]
    public async Task<IActionResult> GetMenuItems(string kitchenId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _catalog.GetItemsAsync(kitchenId, cancellationToken), _logger);

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("get_orders/{kitchenId}")]
    public async Task<IActionResult> GetOrders(string kitchenId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _orders.GetKitchenOrdersAsync(kitchenId, _currentUser.UserId, cancellationToken), _logger);

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("accept_order/{kitchenId}/{orderId}")]
    public async Task<IActionResult> AcceptOrder(string kitchenId, string orderId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _orders.AcceptAsync(kitchenId, orderId, _currentUser.UserId, cancellationToken), _logger);

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("reject_order/{kitchenId}/{orderId}")]
    public async Task<IActionResult> RejectOrder(string kitchenId, string orderId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _orders.RejectAsync(kitchenId, orderId, _currentUser.UserId, cancellationToken), _logger);

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("update_order_status/{kitchenId}/{orderId}")]
    public async Task<IActionResult> UpdateOrderStatus(string kitchenId, string orderId, [FromQuery] string status, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _orders.UpdateKitchenStatusAsync(kitchenId, orderId, _currentUser.UserId, status, cancellationToken), _logger);

    [Authorize(Roles = Roles.KitchenOwner)]
    [HttpPost]
    [Route("get_ledger/{kitchenId}")]
    public async Task<IActionResult> GetLedger(string kitchenId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _settlements.GetKitchenLedgerAsync(kitchenId, _currentUser.UserId, cancellationToken), _logger);
}
