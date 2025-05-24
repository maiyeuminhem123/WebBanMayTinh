using System;
using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models
{
    public class Voucher
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        public decimal? DiscountAmount { get; set; }

        public double? DiscountPercent { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        public int? Quantity { get; set; }

        // Giá trị đơn hàng tối thiểu để áp dụng voucher (tuỳ chọn)
        [Range(0, double.MaxValue)]
        public decimal? MinOrderValue { get; set; }
    }
}
