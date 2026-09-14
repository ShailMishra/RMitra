using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RM.DataModel.Kitchen
{
    public class ResubmitDocumentsRequest
    {
        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile number must be a valid 10-digit Indian mobile number.")]
        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; } = string.Empty;

        [JsonProperty("panCard")]
        public string? PanCard { get; set; }

        [JsonProperty("kitchenPhoto")]
        public string? KitchenPhoto { get; set; }

        [JsonProperty("documents")]
        public List<ResubmittedDocument> Documents { get; set; } = new();
    }

    public class ResubmittedDocument
    {
        [Required]
        [JsonProperty("documentType")]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [JsonProperty("documentUrl")]
        public string DocumentUrl { get; set; } = string.Empty;
    }
}
