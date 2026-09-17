using System.Data;
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
        var next = await db.ExecuteScalarAsync<int>(
            "uspNextPublicId",
            new { Prefix = prefix },
            commandType: CommandType.StoredProcedure);

        return $"{prefix}{next}";
    }
}
