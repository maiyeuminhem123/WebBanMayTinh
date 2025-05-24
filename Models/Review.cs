using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên của bạn")]
        public string? UserName { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}