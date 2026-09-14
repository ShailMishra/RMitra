using Microsoft.Extensions.Configuration;

namespace RM.DataRepository.CommonRequests.Helper
{
    public static class ConfigurationHelper
    {
        public static IConfiguration Config { get; private set; } = null!;

        public static void Initialize(IConfiguration configuration)
        {
            Config = configuration;
        }
    }
}
