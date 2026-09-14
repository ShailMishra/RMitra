using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMitra.Api.ExtensionMethod;
using RMitra.Application.Identity;

namespace RMitra.Api.Controllers;

[AllowAnonymous]
[Route("/api/homely/auth")]
[Tags("Authentication")]
public class AuthController : ApiControllerBase
{
    private readonly IIdentityService _identity;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IIdentityService identity, ILogger<AuthController> logger)
    {
        _identity = identity;
        _logger = logger;
    }

    [HttpPost]
    [Route("send_otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _identity.SendOtpAsync(request, cancellationToken), _logger, "OTP sent successfully.");
    }

    [HttpPost]
    [Route("verify_otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _identity.VerifyOtpAsync(request, cancellationToken), _logger, "OTP verified successfully.");
    }

    [HttpPost]
    [Route("set_password")]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        await _identity.SetPasswordAsync(request, cancellationToken);
        return this.OkCustom(request, new { message = "Password saved." }, _logger);
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return this.BadRequestCustom(request, null, _logger);

        return this.OkCustom(request, await _identity.LoginAsync(request, cancellationToken), _logger);
    }
}
