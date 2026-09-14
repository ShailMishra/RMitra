using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RM.DataModel.MobileVerification
{
    public class SendOtpRequest
    {
        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile number must be a valid 10-digit Indian mobile number.")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; } = string.Empty;
    }
}
