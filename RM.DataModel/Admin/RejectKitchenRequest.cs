using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RM.DataModel.Admin
{
    public class RejectKitchenRequest
    {
        [Required]
        [JsonProperty("reason")]
        public string Reason { get; set; } = string.Empty;
    }
}
