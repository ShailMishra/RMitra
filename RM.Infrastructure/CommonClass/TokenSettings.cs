namespace RM.Infrastructure.CommonClass
{
    public class TokenSettings
    {
        public string StoreCode { get; set; } = string.Empty;

        public string API_Key { get; set; } = string.Empty;

        public string Secret_Key { get; set; } = string.Empty;

        public string CheckSumSecretKey { get; set; } = string.Empty;

        public string CheckSumFlag { get; set; } = "N";
    }
}
