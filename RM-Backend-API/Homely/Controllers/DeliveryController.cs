using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMitra.Api.ExtensionMethod;
using RMitra.Api.Uploads;
using RMitra.Application.Delivery;
using RMitra.Application.Settlement;
using RMitra.BuildingBlocks.Security;

namespace RMitra.Api.Controllers;

[Route("/api/homely/rider")]
[Tags("Rider")]
public class RiderController : ApiControllerBase
{
    private readonly IRiderService _riders;
    private readonly ISettlementService _settlements;
    private readonly ICurrentUser _currentUser;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<RiderController> _logger;

    public RiderController(
        IRiderService riders,
        ISettlementService settlements,
        ICurrentUser currentUser,
        IWebHostEnvironment env,
        ILogger<RiderController> logger)
    {
        _riders = riders;
        _settlements = settlements;
        _currentUser = currentUser;
        _env = env;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRiderRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _riders.RegisterAsync(request, cancellationToken), _logger);
    }

    [Authorize(Roles = Roles.Rider)]
    [HttpPost]
    [Route("upload_document")]
    public async Task<IActionResult> UploadDocument([FromForm] string documentType, IFormFile file, CancellationToken cancellationToken)
    {
        var url = await LocalFileStore.SaveAsync(_env, file, cancellationToken);
        await _riders.AddDocumentAsync(_currentUser.UserId, documentType, url, cancellationToken);
        return this.OkCustom(null, new { documentType, fileUrl = url }, _logger);
    }

    [Authorize(Roles = Roles.Rider)]
    [HttpPost]
    [Route("submit")]
    public async Task<IActionResult> Submit(CancellationToken cancellationToken)
    {
        await _riders.SubmitAsync(_currentUser.UserId, cancellationToken);
        return this.OkCustom(null, new { status = "SUBMITTED" }, _logger);
    }

    [Authorize(Roles = Roles.Rider)]
    [HttpPost]
    [Route("set_presence")]
    public async Task<IActionResult> SetPresence([FromBody] PresenceRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        await _riders.SetPresenceAsync(_currentUser.UserId, request, cancellationToken);
        return this.OkCustom(request, request, _logger);
    }

    [Authorize(Roles = Roles.Rider)]
    [HttpPost]
    [Route("get_current_assignment")]
    public async Task<IActionResult> GetCurrentAssignment(CancellationToken cancellationToken) =>
        this.OkCustom(null, await _riders.GetCurrentAssignmentAsync(_currentUser.UserId, cancellationToken), _logger);

    [Authorize(Roles = Roles.Rider)]
    [HttpPost]
    [Route("respond_assignment/{assignmentId}")]
    public async Task<IActionResult> RespondAssignment(string assignmentId, [FromBody] RespondAssignmentRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _riders.RespondAsync(_currentUser.UserId, assignmentId, request, cancellationToken), _logger);
    }

    [Authorize(Roles = Roles.Rider)]
    [HttpPost]
    [Route("update_trip_status/{orderId}")]
    public async Task<IActionResult> UpdateTripStatus(string orderId, [FromQuery] string status, CancellationToken cancellationToken)
    {
        await _riders.UpdateTripStatusAsync(_currentUser.UserId, orderId, status, cancellationToken);
        return this.OkCustom(null, new { orderId, status }, _logger);
    }

    [Authorize(Roles = Roles.Rider)]
    [HttpPost]
    [Route("get_ledger")]
    public async Task<IActionResult> GetLedger(CancellationToken cancellationToken) =>
        this.OkCustom(null, await _settlements.GetRiderLedgerAsync(_currentUser.UserId, cancellationToken), _logger);
}
