using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RM.DataModel.Kitchen
{
    public class KitchenBankDetails
    {
        [Required]
        [JsonProperty("accountHolderName")]
        public string AccountHolderName { get; set; } = string.Empty;

        [Required]
        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [JsonProperty("ifscCode")]
        public string IfscCode { get; set; } = string.Empty;

        [Required]
        [JsonProperty("bankName")]
        public string BankName { get; set; } = string.Empty;
    }
}
