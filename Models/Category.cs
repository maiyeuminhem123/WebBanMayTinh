using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Đảm bảo có using này nếu chưa có

namespace WebBanMayTinh.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        [Required, StringLength(50)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public int? ParentId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category>? SubCategories { get; set; }

        public List<Product>? Products { get; set; }
    }
}