using Newtonsoft.Json;

namespace RM.DataModel.MobileVerification
{
    public class VerifyOtpResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("verificationToken")]
        public string VerificationToken { get; set; } = string.Empty;
    }
}
