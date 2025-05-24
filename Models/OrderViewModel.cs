namespace WebBanMayTinh.Models
{
    public class OrderViewModel
    {
        public string? Message { get; set; }
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }
}