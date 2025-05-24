namespace WebBanMayTinh.Models
{
    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
