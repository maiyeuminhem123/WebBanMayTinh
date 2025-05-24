using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WebBanMayTinh.Models;
using WebBanMayTinh.Repositories;

namespace WebBanMayTinh.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        // Chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.UserId != userId)
            {
                return Forbid();
            }

            return View(order);
        }

        // Lịch sử đơn hàng
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return View(orders);
        }

        // Hoàn tất đơn hàng
        public async Task<IActionResult> Complete(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            if (order.UserId != userId)
            {
                return Forbid();
            }

            return View(order);
        }

        // Quá trình vận chuyển
        public async Task<IActionResult> Track(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("History");
            }

            if (order.UserId != userId)
            {
                TempData["Error"] = "Bạn không có quyền xem thông tin vận chuyển của đơn hàng này.";
                return RedirectToAction("History");
            }

            return View(order);
        }
    }
}