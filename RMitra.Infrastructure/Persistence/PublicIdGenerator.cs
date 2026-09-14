using Dapper;
using RMitra.Application.Abstractions;

namespace RMitra.Infrastructure.Persistence;

public class PublicIdGenerator : IPublicIdGenerator
{
    private readonly ISqlConnectionFactory _connections;

    public PublicIdGenerator(ISqlConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<string> NextAsync(string prefix, CancellationToken cancellationToken = default)
    {
        using var db = _connections.Create();
        await db.ExecuteAsync(
            @"IF NOT EXISTS (SELECT 1 FROM core.NumberSeries WHERE Prefix = @prefix)
              INSERT INTO core.NumberSeries (Prefix, LastNumber) VALUES (@prefix, 10000);",
            new { prefix });

        var next = await db.ExecuteScalarAsync<int>(
            @"UPDATE core.NumberSeries
              SET LastNumber = LastNumber + 1
              OUTPUT INSERTED.LastNumber
              WHERE Prefix = @prefix;",
            new { prefix });

        return $"{prefix}{next}";
    }
}
