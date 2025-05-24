namespace WebBanMayTinh.Models
{
    public class WarrantyResultViewModel
    {
        public string? Name { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }
        public bool IsExpired { get; set; }
    }
}
