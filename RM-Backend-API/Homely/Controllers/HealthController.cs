using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RMitra.Api.ExtensionMethod;
using RMitra.Application.Abstractions;

namespace RMitra.Api.Controllers;

[AllowAnonymous]
[Route("/api/homely/health")]
[Tags("Health")]
public class HealthController : ApiControllerBase
{
    private readonly ISqlConnectionFactory _connections;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ISqlConnectionFactory connections, ILogger<HealthController> logger)
    {
        _connections = connections;
        _logger = logger;
    }

    [HttpPost]
    [Route("ping")]
    public IActionResult Ping() =>
        this.OkCustom(null, new { status = "Healthy", product = "HOMELY", utc = DateTime.UtcNow }, _logger);

    [HttpPost]
    [Route("ready")]
    public IActionResult Ready()
    {
        using var db = _connections.Create();
        db.Open();
        return this.OkCustom(null, new { status = "Ready", database = "RasoiMitra", utc = DateTime.UtcNow }, _logger);
    }
}
