namespace janaez.webapi.Models
{
    public class TodayFuneralDetailDto
    {
        public string DeadName { get; set; }
        public int prayerId { get; set; }
        public string prayerName { get; set; }
        public int placeId { get; set; }
        public string placeName { get; set; }
        public string SexId { get; set; }
        public string SexName { get; set; }
        public string? AmbulanceNo { get; set; }
        public string? GraveNo { get; set; }
        public string? Notes { get; set; }
    }
}
