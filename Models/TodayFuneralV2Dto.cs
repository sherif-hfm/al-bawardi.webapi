using System.Text.Json.Serialization;

namespace janaez.webapi.Models
{
    public class TodayFuneralV2Dto
    {
        public int prayerId { get; set; }
        public string prayerName { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? men { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? women { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? children { get; set; }
    }
}
