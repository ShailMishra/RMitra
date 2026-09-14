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
                "UPDATE mstUsers SET PasswordHash=@hash, UpdatedAt=SYSUTCDATETIME() WHERE MobileNumber='9999999999' AND Role='ADMIN' AND PasswordHash IS NULL",
                new { hash = hasher.Hash(password) });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Admin password seed skipped. Create the HomelyFood database first.");
        }
    }
}
