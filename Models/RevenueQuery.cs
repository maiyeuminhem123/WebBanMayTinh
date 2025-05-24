namespace WebBanMayTinh.Models
{
    public class RevenueQuery
    {
        public string ReportType { get; set; } = "day";   // day | month | year
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Category { get; set; } = string.Empty; // "all" = không lọc
    }
}
