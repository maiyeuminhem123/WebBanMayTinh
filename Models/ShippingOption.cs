using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models
{
    public class ShippingOption
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Phương thức vận chuyển")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Phí vận chuyển")]
        public decimal Fee { get; set; }

        [Display(Name = "Thời gian dự kiến (ngày)")]
        public int EstimatedDays { get; set; }
    }
}
