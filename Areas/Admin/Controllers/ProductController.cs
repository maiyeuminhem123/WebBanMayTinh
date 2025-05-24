using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebBanMayTinh.Models;
using WebBanMayTinh.Repositories;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IReviewRepository _reviewRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IReviewRepository reviewRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _reviewRepository = reviewRepository;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        [AllowAnonymous] 
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }
        [Authorize(Roles = "Admin ,Employee")]
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            if (!categories.Any())
            {
                ModelState.AddModelError("", "No categories available. Please add categories first.");
            }
            ViewBag.Categories = new SelectList(categories, "CategoryID", "Name");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Add(Product product, IFormFile? uploadedImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadedImage != null)
                {
                    product.ImageUrl = await SaveImage(uploadedImage);
                }

                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryID", "Name");
            return View(product);
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryID", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            ModelState.Remove("ImageUrl"); // Bỏ validate ảnh nếu không bắt buộc
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null) return NotFound();

                // Ánh xạ tất cả các thuộc tính
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl; // Sử dụng ImageUrl từ form
                existingProduct.DetailDescription = product.DetailDescription;
                existingProduct.CPU = product.CPU;
                existingProduct.GPU = product.GPU;
                existingProduct.RAM = product.RAM;
                existingProduct.Storage = product.Storage;
                existingProduct.OperatingSystem = product.OperatingSystem;
                existingProduct.Brand = product.Brand;

                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, trả lại View với danh sách danh mục
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryID", "Name", product.CategoryId);
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Lưu ảnh
        private async Task<string> SaveImage(IFormFile image)
        {
            var filePath = Path.Combine("wwwroot/images", image.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            return "/images/" + image.FileName;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddReview(int productId, string userName, int rating, string comment)
        {
            if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(userName)) return BadRequest();

            var review = new Review
            {
                ProductId = productId,
                UserName = userName,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return NotFound();
            product.Reviews ??= new List<Review>();
            product.Reviews.Add(review);
            await _productRepository.UpdateAsync(product);
            return RedirectToAction(nameof(Display), new { id = productId });
        }

    }
}