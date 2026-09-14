using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RM.DataModel.Kitchen
{
    public class KitchenAddress
    {
        [Required]
        [JsonProperty("line1")]
        public string Line1 { get; set; } = string.Empty;

        [JsonProperty("line2")]
        public string? Line2 { get; set; }

        [Required]
        [JsonProperty("city")]
        public string City { get; set; } = string.Empty;

        [Required]
        [JsonProperty("state")]
        public string State { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be a 6-digit number.")]
        [JsonProperty("pincode")]
        public string Pincode { get; set; } = string.Empty;
    }
}
