using RM.DataRepository.DBDapper;

namespace RM.DataRepository.Health
{
    public interface IHealthRepository
    {
        bool TestDatabaseConnection();
    }

    public class HealthRepository : IHealthRepository
    {
        private readonly DapperContext _context;

        public HealthRepository(DapperContext context)
        {
            _context = context;
        }

        public bool TestDatabaseConnection()
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.ExecuteScalar();
            return true;
        }
    }
}
