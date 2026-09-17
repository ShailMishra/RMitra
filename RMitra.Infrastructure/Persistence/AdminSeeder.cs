using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RMitra.Application.Abstractions;

namespace RMitra.Infrastructure.Persistence;

public static class AdminSeeder
{
    public static async Task TrySeedAsync(ISqlConnectionFactory connections, IPasswordHasher hasher, IConfiguration configuration, ILogger logger)
    {
        try
        {
            var password = configuration["Admin:Password"] ?? "Admin@123";
            using var db = connections.Create();
            await db.ExecuteAsync(
                "uspSeedAdminPassword",
                new { PasswordHash = hasher.Hash(password) },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Admin password seed skipped. Create the RasoiMitra database first.");
        }
    }
}
