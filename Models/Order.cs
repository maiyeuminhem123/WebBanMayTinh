namespace WebBanMayTinh.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? CategoryID { get; set; }
        public int CustomerId { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public List<OrderStatusHistory>? StatusHistory { get; set; }
    }
}