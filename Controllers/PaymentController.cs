using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebBanMayTinh.Extensions;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.VNPAY;

namespace WebBanMayTinh.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVnPayService _vnPayService;

        public PaymentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IVnPayService vnPayService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _vnPayService = vnPayService ?? throw new ArgumentNullException(nameof(vnPayService));
        }

        [HttpGet] // Đổi thành GET để nhận redirect từ ShoppingCartController
        public async Task<IActionResult> ProcessVNPay()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart?.Items?.Any() != true)
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var pendingOrder = HttpContext.Session.GetObjectFromJson<WebBanMayTinh.Models.Order>("PendingOrder");
            if (pendingOrder == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đơn hàng.";
                return RedirectToAction("Checkout", "ShoppingCart");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var order = new WebBanMayTinh.Models.Order
            {
                CustomerName = pendingOrder.CustomerName ?? string.Empty, // Fix for CS8601
                Email = pendingOrder.Email ?? string.Empty, // Fix for CS8601
                PhoneNumber = pendingOrder.PhoneNumber ?? string.Empty, // Fix for CS8601
                ShippingAddress = pendingOrder.ShippingAddress ?? string.Empty, // Fix for CS8601
                Notes = pendingOrder.Notes ?? string.Empty, // Fix for CS8601
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity),
                Status = "Pending",
                OrderCode = pendingOrder.OrderCode ?? string.Empty, // Fix for CS8601
                PaymentMethod = "VNPay"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Order saved: OrderCode={order.OrderCode}, Id={order.Id}");

            if (string.IsNullOrEmpty(order.OrderCode))
            {
                TempData["Error"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var paymentModel = new VnPaymentRequestModel
            {
                OrderId = order.OrderCode, // Ensure OrderCode is a string and not null
                Amount = (double)order.TotalAmount,
                Description = $"Thanh toán đơn hàng {order.OrderCode}",
                CreatedDate = DateTime.Now,
                FullName = order.CustomerName // Fix for CS8601
            };

            var paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, paymentModel);
            return Redirect(paymentUrl);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnPayService.PaymentExcute(Request.Query);
            var orderCode = response.OrderId;
            var userId = _userManager.GetUserId(User);
            Console.WriteLine($"Looking for OrderCode: {orderCode}, UserId: {userId}");

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode && o.UserId == userId);
            if (order == null)
            {
                Console.WriteLine($"Order not found for OrderCode: {orderCode}, UserId: {userId}");
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (response.Success)
            {
                order.Status = "Confirmed";
                order.OrderDetails = cart?.Items?.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Price
                }).ToList() ?? new List<OrderDetail>();

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("Cart");
                HttpContext.Session.Remove("PendingOrder");

                TempData["Success"] = "Thanh toán thành công!";
                return RedirectToAction("OrderCompleted", "ShoppingCart", new { orderId = order.Id });
            }
            else
            {
                order.Status = response.VnPayResponseCode == "24" ? "Canceled" : "Payment Failed";
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                TempData["Error"] = $"Thanh toán thất bại: {response.OrderDescription}";
                return RedirectToAction("Index", "ShoppingCart");
            }
        }
    }
}