using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RMitra.Application.Abstractions;

namespace RMitra.Infrastructure.Persistence;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = configuration.GetConnectionString("RMitra");
        }

        _connectionString = connectionString?.Trim() ?? string.Empty;
    }

    public IDbConnection Create()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "SQL connection string is not configured. Set ConnectionStrings:ConnectionString to Azure SQL (database RasoiMitra) and run database/Install_All.sql.");
        }

        return new SqlConnection(_connectionString);
    }
}
