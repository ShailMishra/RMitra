using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace RM.DataRepository.DBDapper
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = SqlConnectionSettings.ResolveConnectionString(configuration);
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

        public IDbConnection CreateConnection()
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException(
                    "SQL connection string is not configured. Set ConnectionStrings__ConnectionString on Render when Azure SQL is ready.");
            }

            return new SqlConnection(_connectionString);
        }

        public SqlConnection CreateSqlConnection() => (SqlConnection)CreateConnection();

        public T Get<T>(string query, DynamicParameters parameters, CommandType commandType = CommandType.Text)
        {
            using IDbConnection db = CreateConnection();
            return db.Query<T>(query, parameters, commandType: commandType).FirstOrDefault();
        }

        public List<T> GetAll<T>(string query, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = CreateConnection();
            return db.Query<T>(query, parameters, commandType: commandType).ToList();
        }

        public T Insert<T>(string query, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = CreateConnection();
            if (db.State == ConnectionState.Closed)
                db.Open();

            using var transaction = db.BeginTransaction();
            try
            {
                var result = db.Query<T>(query, parameters, commandType: commandType, transaction: transaction).FirstOrDefault();
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public T Update<T>(string query, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            return Insert<T>(query, parameters, commandType);
        }
    }

    public class DapperContextViewOnly
    {
        private readonly string _connectionString;

        public DapperContextViewOnly(IConfiguration configuration)
        {
            var viewOnly = configuration.GetConnectionString("HPCLViewOnlyConnectionString");
            _connectionString = string.IsNullOrWhiteSpace(viewOnly)
                ? SqlConnectionSettings.ResolveConnectionString(configuration)
                : viewOnly;
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

        public IDbConnection CreateConnection()
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException(
                    "SQL connection string is not configured. Set ConnectionStrings__ConnectionString on Render when Azure SQL is ready.");
            }

            return new SqlConnection(_connectionString);
        }

        public SqlConnection CreateSqlConnection() => (SqlConnection)CreateConnection();
    }

    internal static class SqlConnectionSettings
    {
        public static string ResolveConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = configuration.GetConnectionString("HPCLConnectionString");
            }

            return connectionString?.Trim() ?? string.Empty;
        }
    }
}
