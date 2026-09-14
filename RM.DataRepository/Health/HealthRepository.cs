using RM.DataRepository.ExcelDb;

namespace RM.DataRepository.Health
{
    public interface IHealthRepository
    {
        bool TestDatabaseConnection();
    }

    public class HealthRepository : IHealthRepository
    {
        private readonly IExcelKitchenStore _excelStore;

        public HealthRepository(IExcelKitchenStore excelStore)
        {
            _excelStore = excelStore;
        }

        public bool TestDatabaseConnection()
        {
            return _excelStore.CanConnect();
        }
    }
}
