namespace RM.Infrastructure.CommonClass
{
    public class TokenSettings
    {
        public string StoreCode { get; set; } = string.Empty;

        public string API_Key { get; set; } = string.Empty;

        public string Secret_Key { get; set; } = string.Empty;

        public string Homely_API_Key { get; set; } = string.Empty;

        public string Homely_Secret_Key { get; set; } = string.Empty;

        public bool Matches(string? apiKey, string? secretKey)
        {
            return IsPair(apiKey, secretKey, API_Key, Secret_Key)
                || IsPair(apiKey, secretKey, Homely_API_Key, Homely_Secret_Key);
        }

        private static bool IsPair(string? apiKey, string? secretKey, string expectedApiKey, string expectedSecretKey)
        {
            return !string.IsNullOrWhiteSpace(expectedApiKey)
                && !string.IsNullOrWhiteSpace(expectedSecretKey)
                && string.Equals(apiKey, expectedApiKey, StringComparison.Ordinal)
                && string.Equals(secretKey, expectedSecretKey, StringComparison.Ordinal);
        }

        public string CheckSumSecretKey { get; set; } = string.Empty;

        public string CheckSumFlag { get; set; } = "N";
    }
}
