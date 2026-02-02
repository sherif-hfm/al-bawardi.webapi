using System.Text.Json.Serialization;

namespace janaez.webapi.Models
{
    public class Sex
    {
        public string id { get; set; }
        
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        
    }
}
