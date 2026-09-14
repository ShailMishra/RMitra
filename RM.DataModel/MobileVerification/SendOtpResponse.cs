using Newtonsoft.Json;

namespace RM.DataModel.MobileVerification
{
    public class SendOtpResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; } = string.Empty;
    }
}
