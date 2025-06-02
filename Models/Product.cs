using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string? Name { get; set; }
        [Range(1, 99999999)]
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageUrl2 { get; set; } // Ảnh phụ 1
        public string? ImageUrl3 { get; set; } // Ảnh phụ 2
        public List<ProductImage>? Images { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? DetailDescription { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int WarrantyPeriodMonths { get; set; }



        public string? CPU { get; set; } // Ví dụ: "Intel Core i7-12700H"
        public string? GPU { get; set; } // Ví dụ: "NVIDIA RTX 3060"
        public string? RAM { get; set; } // Ví dụ: "16GB DDR4"
        public string? Storage { get; set; } // Ví dụ: "1TB SSD"
        public string? OperatingSystem { get; set; } // Ví dụ: "Windows 11"
        public string? Brand { get; set; } // Ví dụ: "ASUS", "Dell"
        public List<Review>? Reviews { get; set; } // Danh sách đánh giá từ khách hàng
    }
}
