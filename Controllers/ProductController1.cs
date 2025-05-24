using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Models;
using WebBanMayTinh.Repositories;

namespace WebBanMayTinh.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchQuery)
        {
            var products = await _productRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                // Lọc sản phẩm dựa trên từ khóa
                searchQuery = searchQuery.ToLower();
                products = products.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(searchQuery)) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(searchQuery))
                ).ToList();
            }

            return View(products);
        }
    }
}