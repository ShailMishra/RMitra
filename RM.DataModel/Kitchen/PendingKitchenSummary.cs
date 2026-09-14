using Newtonsoft.Json;

namespace RM.DataModel.Kitchen
{
    public class PendingKitchenSummary
    {
        [JsonProperty("kitchenId")]
        public string KitchenId { get; set; } = string.Empty;

        [JsonProperty("kitchenName")]
        public string KitchenName { get; set; } = string.Empty;

        [JsonProperty("ownerName")]
        public string OwnerName { get; set; } = string.Empty;

        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("kitchenType")]
        public string KitchenType { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("submittedAt")]
        public DateTime SubmittedAt { get; set; }
    }
}
