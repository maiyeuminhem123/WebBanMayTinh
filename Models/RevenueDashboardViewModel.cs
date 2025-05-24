namespace WebBanMayTinh.Models
{
    public class RevenueDashboardViewModel
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Category { get; set; } = string.Empty;

        public IReadOnlyList<RevenueData> ChartData { get; set; } = new List<RevenueData>();
        public IReadOnlyList<CategoryDistribution> CategoryData { get; set; } = new List<CategoryDistribution>();
        public RevenueSummary Summary { get; set; } = new RevenueSummary();
    }
}
