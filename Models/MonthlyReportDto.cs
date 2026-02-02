namespace janaez.webapi.Models
{
    public class MonthlyReportDto
    {
        public int year { get; set; }

        public int monthNo { get; set; }
        
        public string month { get; set; }

        public int boy { get; set; }

        public int woman { get; set; }

        public int girl { get; set; }

        public int man { get; set; }

        public int total { get; set; }
    }
}
