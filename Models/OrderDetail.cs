namespace WebBanMayTinh.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; } // Khóa ngoại, bắt buộc
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public Order? Order { get; set; } // Quan hệ điều hướng
        public Product? Product { get; set; }
    }
}