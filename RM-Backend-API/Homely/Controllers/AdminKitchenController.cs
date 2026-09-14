using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMitra.Api.ExtensionMethod;
using RMitra.Application.Delivery;
using RMitra.Application.Kitchen;
using RMitra.Application.Settlement;
using RMitra.BuildingBlocks.Security;

namespace RMitra.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("/api/homely/admin")]
[Tags("Admin")]
public class AdminController : ApiControllerBase
{
    private readonly IKitchenService _kitchens;
    private readonly IRiderService _riders;
    private readonly ISettlementService _settlements;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IKitchenService kitchens, IRiderService riders, ISettlementService settlements, ILogger<AdminController> logger)
    {
        _kitchens = kitchens;
        _riders = riders;
        _settlements = settlements;
        _logger = logger;
    }

    [HttpPost]
    [Route("get_pending_kitchens")]
    public async Task<IActionResult> GetPendingKitchens(CancellationToken cancellationToken) =>
        this.OkCustom(null, await _kitchens.GetPendingAsync(cancellationToken), _logger);

    [HttpPost]
    [Route("approve_kitchen/{kitchenId}")]
    public async Task<IActionResult> ApproveKitchen(string kitchenId, CancellationToken cancellationToken) =>
        this.OkCustom(null, await _kitchens.ApproveAsync(kitchenId, cancellationToken), _logger);

    [HttpPost]
    [Route("reject_kitchen/{kitchenId}")]
    public async Task<IActionResult> RejectKitchen(string kitchenId, [FromBody] RejectKitchenRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _kitchens.RejectAsync(kitchenId, request, cancellationToken), _logger);
    }

    [HttpPost]
    [Route("request_documents/{kitchenId}")]
    public async Task<IActionResult> RequestDocuments(string kitchenId, [FromBody] RequestDocumentsRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _kitchens.RequestDocumentsAsync(kitchenId, request, cancellationToken), _logger);
    }

    [HttpPost]
    [Route("review_rider/{riderId}")]
    public async Task<IActionResult> ReviewRider(string riderId, [FromBody] RiderReviewRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        await _riders.ReviewAsync(riderId, request, cancellationToken);
        return this.OkCustom(request, new { riderId, request.Decision }, _logger);
    }

    [HttpPost]
    [Route("get_settlement_config")]
    public IActionResult GetSettlementConfig() =>
        this.OkCustom(null, _settlements.GetConfig(), _logger);

    [HttpPost]
    [Route("run_settlements")]
    public async Task<IActionResult> RunSettlements(CancellationToken cancellationToken) =>
        this.OkCustom(null, await _settlements.RunWeeklyAsync(cancellationToken), _logger);
}
