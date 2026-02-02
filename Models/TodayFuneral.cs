using System.Text.Json.Serialization;

namespace janaez.webapi.Models
{
    public class TodayFuneral
    {
        public int prayerId { get; set; }
        public string prayerName { get; set; }

        [JsonPropertyName("M")]
        public int M { get; set; }

        [JsonPropertyName("F")]
        public int F { get; set; }

        [JsonPropertyName("B")]
        public int B { get; set; }

        [JsonPropertyName("G")]
        public int G { get; set; }

        [JsonPropertyName("Total")]
        public int Total { get; set; }
    }
}
