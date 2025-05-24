namespace WebBanMayTinh.Models
{
    public class RevenueSummary
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public double GrowthRate { get; set; }
    }
}
