using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Repositories;

namespace WebBanMayTinh.ViewComponents
{
    public class MegaCategoryMenuViewComponent : ViewComponent
    {
        // Giả sử m có service để lấy danh mục
        private readonly ICategoryService _categoryService;

        public MegaCategoryMenuViewComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.GetCategoriesAsync();
            return View(categories);
        }
    }
}
