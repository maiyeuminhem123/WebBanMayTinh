using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Repositories
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetCategoriesAsync() // Thay đổi kiểu trả về thành List<Category>
        {
            return await _context.Categories
                .Include(c => c.SubCategories) // Đảm bảo lấy danh mục con
                .Where(c => c.ParentId == null) // Chỉ lấy danh mục cha
                .ToListAsync();
        }
    }
}