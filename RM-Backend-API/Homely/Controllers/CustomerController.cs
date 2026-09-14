using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMitra.Api.ExtensionMethod;
using RMitra.Application.Customer;
using RMitra.BuildingBlocks.Security;

namespace RMitra.Api.Controllers;

[Route("/api/homely/customer")]
[Tags("Customer")]
public class CustomerController : ApiControllerBase
{
    private readonly ICustomerService _customers;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customers, ICurrentUser currentUser, ILogger<CustomerController> logger)
    {
        _customers = customers;
        _currentUser = currentUser;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _customers.RegisterAsync(request, cancellationToken), _logger);
    }

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [Route("get_me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken) =>
        this.OkCustom(null, await _customers.GetMeAsync(_currentUser.UserId, cancellationToken), _logger);

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [Route("update_me")]
    public async Task<IActionResult> UpdateMe([FromBody] PatchCustomerRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _customers.PatchMeAsync(_currentUser.UserId, request, cancellationToken), _logger);
    }

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [Route("get_addresses")]
    public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken) =>
        this.OkCustom(null, await _customers.GetAddressesAsync(_currentUser.UserId, cancellationToken), _logger);

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    [Route("add_address")]
    public async Task<IActionResult> AddAddress([FromBody] UpsertAddressRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _customers.AddAddressAsync(_currentUser.UserId, request, cancellationToken), _logger);
    }
}
