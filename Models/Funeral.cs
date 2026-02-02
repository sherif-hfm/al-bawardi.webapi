using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace janaez.webapi.Models
{
    public class Funeral
    {
        public int Id { get; set; }
        public string DeadName { get; set; }
        
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateTime Date { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Foreign Keys
        [Column("prayerId")]
        public int PrayerId { get; set; }
        
        [Column("sexId")]
        public string SexId { get; set; }

        [Column("purialPlaceId")]
        [JsonPropertyName("purialplaceId")]
        public int PurialPlaceId { get; set; }

        // Navigation Properties
        public Prayer Prayer { get; set; }

        [ForeignKey("SexId")]
        public Sex Sex { get; set; }
        
        [JsonPropertyName("purialplace")]
        public PurialPlace PurialPlace { get; set; }
    }
}
