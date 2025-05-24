namespace WebBanMayTinh.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string? Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Notes { get; set; }
        public Order? Order { get; set; }
    }
}

