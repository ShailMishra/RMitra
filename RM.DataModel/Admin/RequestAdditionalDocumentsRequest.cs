using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RM.DataModel.Admin
{
    public class RequestAdditionalDocumentsRequest
    {
        [Required]
        [JsonProperty("documentsRequired")]
        public List<string> DocumentsRequired { get; set; } = new();

        [JsonProperty("remarks")]
        public string? Remarks { get; set; }
    }
}
