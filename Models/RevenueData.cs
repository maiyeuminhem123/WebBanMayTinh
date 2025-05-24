namespace WebBanMayTinh.Models
{
    public class RevenueData
    {
        public string Date { get; set; } = string.Empty;   // yyyy‑MM‑dd | yyyy‑MM | yyyy
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public decimal Average { get; set; }
        public double Growth { get; set; }
    }
}
