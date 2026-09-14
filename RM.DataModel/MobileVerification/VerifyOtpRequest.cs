using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RM.DataModel.MobileVerification
{
    public class VerifyOtpRequest
    {
        [Required]
        [JsonProperty("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile number must be a valid 10-digit Indian mobile number.")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be exactly 6 digits.")]
        [JsonProperty("otp")]
        public string Otp { get; set; } = string.Empty;
    }
}
