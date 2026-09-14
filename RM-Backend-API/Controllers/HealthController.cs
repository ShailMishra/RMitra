using RM.DataModel.Common;
using RM.DataRepository.Health;
using RM.Infrastructure.Response;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;
using static RM.Infrastructure.CommonClass.StatusMessage;

namespace RM_Backend_API.Controllers
{
    [ApiController]
    [Route("api/rasoi-mitra")]
    public class HealthController : ControllerBase
    {
        private readonly IHealthRepository _healthRepository;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IHealthRepository healthRepository, ILogger<HealthController> logger)
        {
            _healthRepository = healthRepository;
            _logger = logger;
        }

        [HttpGet("ping")]
        public ActionResult<ApiResponseMessage> Ping()
        {
            var databaseConnected = false;
            try
            {
                databaseConnected = _healthRepository.TestDatabaseConnection();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connectivity check failed.");
            }

            var assembly = Assembly.GetExecutingAssembly().GetName();
            var data = new PingResponse
            {
                Application = assembly.Name ?? "RM-Backend-API",
                Version = assembly.Version?.ToString() ?? "1.0.0",
                TimestampUtc = DateTime.UtcNow,
                DatabaseConnected = databaseConnected
            };

            return Ok(new ApiResponseMessage
            {
                Success = true,
                Status_Code = (int)HttpStatusCode.OK,
                Internel_Status_Code = (int)StatusInformation.Success,
                Message = databaseConnected ? "RM-Backend-API is running and Excel DB is reachable." : "RM-Backend-API is running but Excel DB is not reachable.",
                Method_Name = nameof(Ping),
                Data = data
            });
        }
    }
}
