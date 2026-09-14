using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RM.DataModel.Kitchen
{
    public class KitchenOperatingHours
    {
        [Required]
        [JsonProperty("openTime")]
        public string OpenTime { get; set; } = string.Empty;

        [Required]
        [JsonProperty("closeTime")]
        public string CloseTime { get; set; } = string.Empty;

        [Required]
        [JsonProperty("daysOpen")]
        public List<string> DaysOpen { get; set; } = new();
    }
}
