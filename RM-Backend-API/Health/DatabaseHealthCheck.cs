using RM.DataRepository.DBDapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data;

namespace RM_Backend_API.Health
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly DapperContext _context;

        public DatabaseHealthCheck(DapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!_context.IsConfigured)
            {
                return Task.FromResult(HealthCheckResult.Degraded("Database is not configured yet."));
            }

            try
            {
                using var connection = _context.CreateConnection();
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteScalar();

                return Task.FromResult(HealthCheckResult.Healthy("Database connection OK"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Database connection failed", ex));
            }
        }
    }
}
