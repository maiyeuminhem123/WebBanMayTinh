using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Category
        public IActionResult Index()
        {
            var categories = _context.Categories
                .Include(c => c.ParentCategory) 
                .Include(c => c.SubCategories)
                .ToList();
            return View(categories);
        }

        // GET: Admin/Category/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["Success"] = "Tạo danh mục thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/Category/CreateSub?parentId={id}
        [HttpGet]
        public IActionResult CreateSub(int parentId)
        {
            var parentCategory = _context.Categories.Find(parentId);
            if (parentCategory == null)
            {
                return NotFound();
            }

            var category = new Category
            {
                ParentId = parentId
            };

            ViewBag.ParentName = parentCategory.Name;
            return View(category);
        }

        // POST: Admin/Category/CreateSub
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSub(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["Success"] = "Tạo danh mục con thành công.";
                return RedirectToAction(nameof(Index));
            }

            var parentCategory = _context.Categories.Find(category.ParentId);
            if (parentCategory == null)
            {
                return NotFound();
            }
            ViewBag.ParentName = parentCategory.Name;
            return View(category);
        }

        // GET: Admin/Category/Edit/{id}
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/Category/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Category category)
        {
            if (id != category.CategoryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(category);
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật danh mục thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/Category/Delete/{id}
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/Category/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return NotFound();
            }

            if (category.SubCategories.Any() || category.Products.Any())
            {
                TempData["Error"] = "Không thể xóa danh mục vì nó chứa danh mục con hoặc sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();
            TempData["Success"] = "Xóa danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Category/Details/{id}
        public IActionResult Details(int id)
        {
            var category = _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
    }
}