using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Models;
using System;
using System.Linq;

namespace WebBanMayTinh.Controllers
{
    public class WarrantyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WarrantyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Warranty/Check
        [HttpGet]
        public IActionResult Check()
        {
            return View();
        }

        // POST: Warranty/Check (nếu vẫn muốn hỗ trợ tra cứu dạng submit form truyền thống)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Check(string serialNumber, DateTime purchaseDate)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                ViewBag.Error = "Vui lòng nhập mã serial.";
                return View();
            }

            var product = _context.Products.FirstOrDefault(p => p.SerialNumber == serialNumber);

            if (product == null)
            {
                ViewBag.Error = "Không tìm thấy sản phẩm với mã serial này.";
                return View();
            }

            DateTime expiryDate = purchaseDate.AddYears(2);
            string status = DateTime.Now <= expiryDate ? "Còn bảo hành" : "Hết bảo hành";

            ViewBag.ProductName = product.Name;
            ViewBag.PurchaseDate = purchaseDate.ToString("dd/MM/yyyy");
            ViewBag.ExpiryDate = expiryDate.ToString("dd/MM/yyyy");
            ViewBag.Status = status;

            return View();
        }

        // ✅ AJAX: POST Warranty/CheckAjax
        [HttpPost]
        public JsonResult CheckAjax([FromBody] WarrantyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SerialNumber))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã serial." });
            }

            var product = _context.Products.FirstOrDefault(p => p.SerialNumber == request.SerialNumber);

            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm với mã serial này." });
            }

            DateTime expiryDate = request.PurchaseDate.AddYears(2);
            string status = DateTime.Now <= expiryDate ? "Còn bảo hành" : "Hết bảo hành";

            return Json(new
            {
                success = true,
                productName = product.Name,
                purchaseDate = request.PurchaseDate.ToString("dd/MM/yyyy"),
                expiryDate = expiryDate.ToString("dd/MM/yyyy"),
                status = status
            });
        }
    }

    // ✅ Model cho request từ AJAX
    public class WarrantyRequest
    {
        public string SerialNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
