using Newtonsoft.Json;

namespace RM.DataModel.Admin
{
    public class KitchenApprovalResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("kitchenId")]
        public string KitchenId { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
    }
}
