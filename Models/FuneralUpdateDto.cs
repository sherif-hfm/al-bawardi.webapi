namespace janaez.webapi.Models
{
    public class FuneralUpdateDto
    {
        public int Id { get; set; }
        public string DeadName { get; set; }
        public string Date { get; set; } // Expected in "yyyy-MM-dd"
        public string Sex { get; set; }
        public int PrayerId { get; set; }
        public int PlaceId { get; set; }
    }
}
