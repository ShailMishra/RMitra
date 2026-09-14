using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RM.DataModel.Kitchen
{
    public class KitchenRegistrationRequest
    {
        [Required]
        [JsonProperty("verificationToken")]
        public string VerificationToken { get; set; } = string.Empty;

        [Required]
        [JsonProperty("kitchenName")]
        public string KitchenName { get; set; } = string.Empty;

        [Required]
        [JsonProperty("ownerName")]
        public string OwnerName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile number must be a valid 10-digit Indian mobile number.")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [JsonProperty("address")]
        public KitchenAddress Address { get; set; } = new();

        [Required]
        [JsonProperty("kitchenType")]
        public string KitchenType { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "At least one cuisine type is required.")]
        [JsonProperty("cuisineTypes")]
        public List<string> CuisineTypes { get; set; } = new();

        [Required]
        [JsonProperty("operatingHours")]
        public KitchenOperatingHours OperatingHours { get; set; } = new();

        [Required]
        [JsonProperty("bankDetails")]
        public KitchenBankDetails BankDetails { get; set; } = new();

        [Required]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "PAN Card must be a valid PAN format (e.g. ABCDE1234F).")]
        [JsonProperty("panCard")]
        public string PanCard { get; set; } = string.Empty;

        [Required]
        [JsonProperty("kitchenPhoto")]
        public string KitchenPhoto { get; set; } = string.Empty;
    }
}
