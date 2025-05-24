using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanMayTinh.Extensions;
using WebBanMayTinh.Models;
using WebBanMayTinh.Repositories;

namespace WebBanMayTinh.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PayPalOptions _payPalOptions;

        public ShoppingCartController(
            IProductRepository productRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IOptions<PayPalOptions> payPalOptions)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _payPalOptions = payPalOptions.Value ?? throw new ArgumentNullException(nameof(payPalOptions));
        }

        private PayPalHttpClient GetPayPalClient()
        {
            PayPalEnvironment environment;
            if (_payPalOptions.Mode == "sandbox")
            {
                environment = new SandboxEnvironment(_payPalOptions.ClientId, _payPalOptions.ClientSecret);
            }
            else
            {
                environment = new LiveEnvironment(_payPalOptions.ClientId, _payPalOptions.ClientSecret);
            }

            return new PayPalHttpClient(environment);
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();

            var cartItem = new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                UnitPrice = product.Price,
                Quantity = quantity
            };

            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }

        // Hiển thị giỏ hàng (Bước 1: Giỏ hàng)
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null)
            {
                return NotFound();
            }

            cart.RemoveItem(productId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ hàng
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        [HttpPost]
        public IActionResult UpdateCart(int productId, string? action)
        {
            if (string.IsNullOrEmpty(action))
            {
                return Json(new { success = false, message = "Hành động không hợp lệ." });
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
            }

            switch (action.ToLower())
            {
                case "increase":
                    item.Quantity++;
                    break;

                case "decrease":
                    if (item.Quantity > 1)
                        item.Quantity--;
                    else
                        cart.RemoveItem(productId);
                    break;

                case "remove":
                    cart.RemoveItem(productId);
                    break;

                default:
                    return Json(new { success = false, message = "Hành động không hợp lệ." });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Json(new
            {
                success = true,
                quantity = item?.Quantity ?? 0
            });
        }

        // Hiển thị trang nhập thông tin đặt hàng (Bước 2: Thông tin đặt hàng)
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index");
            }

            var order = new WebBanMayTinh.Models.Order
            {
                TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity)
            };

            // Lấy danh sách Quận/Huyện và Phường/Xã của TP.HCM
            ViewBag.Districts = GetHCMCDistricts();
            ViewBag.Wards = GetWards();

            return View(order);
        }

        // Xử lý khi khách hàng nhập thông tin đặt hàng và nhấn "Đặt hàng ngay"
        [HttpPost]
        public IActionResult Checkout(WebBanMayTinh.Models.Order order, string province, string district, string ward, string street)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Districts = GetHCMCDistricts();
                ViewBag.Wards = GetWards();
                return View(order);
            }

            // Lấy thông tin người dùng
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index");
            }

            // Kết hợp địa chỉ từ các trường
            order.ShippingAddress = $"{street}, {ward}, {district}, {province}";

            // Cập nhật thông tin đơn hàng
            order.UserId = userId;
            order.OrderDate = DateTime.UtcNow;
            order.TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);
            order.Status = "Pending";
            order.OrderCode = "ORD-" + Guid.NewGuid().ToString().Substring(0, 8);

            // Lưu thông tin đơn hàng vào Session
            HttpContext.Session.SetObjectAsJson("PendingOrder", order);

            // Chuyển hướng đến trang hình thức thanh toán
            return RedirectToAction("PaymentMethod");
        }
        [HttpGet]
        public IActionResult PaymentMethod()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index");
            }

            var order = HttpContext.Session.GetObjectFromJson<WebBanMayTinh.Models.Order>("PendingOrder");
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đơn hàng.";
                return RedirectToAction("Checkout");
            }

            // Gán lại TotalAmount từ giỏ hàng
            order.TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);

            // Lưu giỏ hàng vào ViewBag để hiển thị trong View
            ViewBag.Cart = cart;

            return View(order);
        }

        // Xử lý khi khách hàng chọn hình thức thanh toán và nhấn "Đặt hàng ngay"
        [HttpPost]
        public async Task<IActionResult> PaymentMethod(WebBanMayTinh.Models.Order order, string paymentMethod)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index");
            }

            var pendingOrder = HttpContext.Session.GetObjectFromJson<WebBanMayTinh.Models.Order>("PendingOrder");
            if (pendingOrder == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đơn hàng.";
                return RedirectToAction("Checkout");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index");
            }

            // Gán lại thông tin từ pendingOrder
            order.CustomerName = pendingOrder.CustomerName;
            order.Email = pendingOrder.Email;
            order.PhoneNumber = pendingOrder.PhoneNumber;
            order.ShippingAddress = pendingOrder.ShippingAddress;
            order.Notes = pendingOrder.Notes;
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);
            order.Status = "Pending";
            order.OrderCode = pendingOrder.OrderCode;

            // Thêm danh sách sản phẩm từ giỏ hàng
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Price
            }).ToList();

            // Lưu đơn hàng vào database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Xử lý theo hình thức thanh toán
            if (paymentMethod == "cod")
            {
                // Thanh toán khi nhận hàng (COD)
                order.Status = "Pending"; // Trạng thái ban đầu
                await _context.SaveChangesAsync();

                // Xóa session
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.Remove("PendingOrder");

                // Chuyển hướng đến trang hoàn tất
                return RedirectToAction("OrderCompleted", new { orderId = order.Id });
            }
            else if (paymentMethod == "paypal")
            {
                // Thanh toán qua PayPal
                var client = GetPayPalClient();

                var orderRequest = new OrderRequest()
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    PurchaseUnits = new List<PurchaseUnitRequest>()
            {
                new PurchaseUnitRequest()
                {
                    Description = $"Thanh toán đơn hàng #{order.Id}",
                    AmountWithBreakdown = new AmountWithBreakdown()
                    {
                        CurrencyCode = "USD",
                        Value = order.TotalAmount.ToString("F2"),
                        AmountBreakdown = new AmountBreakdown()
                        {
                            ItemTotal = new Money()
                            {
                                CurrencyCode = "USD",
                                Value = order.TotalAmount.ToString("F2")
                            }
                        }
                    },
                    Items = cart.Items.Select(item => new Item()
                    {
                        Name = item.Name,
                        UnitAmount = new Money()
                        {
                            CurrencyCode = "USD",
                            Value = item.Price.ToString("F2")
                        },
                        Quantity = item.Quantity.ToString(),
                        Sku = item.ProductId.ToString()
                    }).ToList()
                }
            },
                    ApplicationContext = new ApplicationContext()
                    {
                        ReturnUrl = Url.Action("Success", "ShoppingCart", null, Request.Scheme),
                        CancelUrl = Url.Action("Cancel", "ShoppingCart", null, Request.Scheme)
                    }
                };

                var request = new OrdersCreateRequest();
                request.Prefer("return=representation");
                request.RequestBody(orderRequest);

                try
                {
                    var response = await client.Execute(request);
                    var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                    var approvalUrl = result.Links.FirstOrDefault(link => link.Rel.Equals("approve", StringComparison.OrdinalIgnoreCase))?.Href;
                    if (approvalUrl == null)
                    {
                        TempData["Error"] = "Không thể tạo thanh toán PayPal: Không tìm thấy URL phê duyệt.";
                        return RedirectToAction("Checkout");
                    }

                    HttpContext.Session.SetString("PayPalOrderId", result.Id);
                    HttpContext.Session.SetInt32("OrderId", order.Id);
                    return Redirect(approvalUrl);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi khi tạo thanh toán PayPal: {ex.Message}";
                    return RedirectToAction("Checkout");
                }
            }
            else
            {
                TempData["Error"] = "Vui lòng chọn hình thức thanh toán.";
                return RedirectToAction("PaymentMethod");
            }
        }

        // Xử lý khi thanh toán PayPal thành công (Bước 4: Hoàn tất)
        [HttpGet]
        public async Task<IActionResult> Success(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                TempData["Error"] = "Thanh toán thất bại: Không tìm thấy Order ID.";
                return RedirectToAction("Checkout");
            }

            var client = GetPayPalClient();

            // Xác nhận đơn hàng
            var request = new OrdersCaptureRequest(orderId);
            request.Prefer("return=representation");

            try
            {
                var response = await client.Execute(request);
                var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                if (result.Status != "COMPLETED")
                {
                    TempData["Error"] = "Thanh toán thất bại: Đơn hàng chưa được hoàn tất.";
                    return RedirectToAction("Checkout");
                }

                var orderIdFromSession = HttpContext.Session.GetInt32("OrderId");
                if (!orderIdFromSession.HasValue)
                {
                    TempData["Error"] = "Thanh toán thất bại: Không tìm thấy Order ID trong session.";
                    return RedirectToAction("Checkout");
                }

                var order = await _context.Orders.FindAsync(orderIdFromSession.Value);
                if (order == null)
                {
                    TempData["Error"] = "Thanh toán thất bại: Không tìm thấy đơn hàng.";
                    return RedirectToAction("Checkout");
                }

                order.Status = "Paid";
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("Cart");
                HttpContext.Session.Remove("PendingOrder");

                // Chuyển hướng đến trang hoàn tất
                return RedirectToAction("OrderCompleted", new { orderId = orderIdFromSession.Value });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Thanh toán thất bại: {ex.Message}";
                return RedirectToAction("Checkout");
            }
        }

        // Trang hoàn tất (Bước 4: Hoàn tất)
        // Hiển thị trang hoàn tất đặt hàng (Bước 4: Hoàn tất)
        [HttpGet]
        public async Task<IActionResult> OrderCompleted(int orderId)
        {
            // Lấy thông tin đơn hàng từ database dựa trên orderId
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // Hiển thị chi tiết đơn hàng
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == _userManager.GetUserId(User));

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Hiển thị lịch sử đơn hàng
        [HttpGet]
        public async Task<IActionResult> OrderHistory()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Xử lý khi khách hàng hủy thanh toán PayPal
        [HttpGet]
        public IActionResult Cancel()
        {
            TempData["Error"] = "Thanh toán đã bị hủy.";
            return RedirectToAction("Checkout");
        }

        // Lấy danh sách Quận/Huyện của TP.HCM
        private List<string> GetHCMCDistricts()
        {
            return new List<string>
            {
                "Quận 1", "Quận 2", "Quận 3", "Quận 4", "Quận 5", "Quận 6", "Quận 7",
                "Quận 8", "Quận 9", "Quận 10", "Quận 11", "Quận 12", "Quận Bình Tân",
                "Quận Bình Thạnh", "Quận Gò Vấp", "Quận Phú Nhuận", "Quận Tân Bình",
                "Quận Tân Phú", "Quận Thủ Đức", "Huyện Bình Chánh", "Huyện Cần Giờ",
                "Huyện Củ Chi", "Huyện Hóc Môn", "Huyện Nhà Bè"
            };
        }

        // Lấy danh sách Phường/Xã theo Quận/Huyện
        private Dictionary<string, List<string>> GetWards()
        {
            return new Dictionary<string, List<string>>
            {
                { "Quận 1", new List<string> { "Phường Bến Nghé", "Phường Bến Thành", "Phường Cầu Kho", "Phường Cầu Ông Lãnh", "Phường Cô Giang", "Phường Đa Kao", "Phường Nguyễn Cư Trinh", "Phường Nguyễn Thái Bình", "Phường Phạm Ngũ Lão", "Phường Tân Định" } },
                { "Quận 2", new List<string> { "Phường An Khánh", "Phường An Lợi Đông", "Phường An Phú", "Phường Bình An", "Phường Bình Khánh", "Phường Bình Trưng Đông", "Phường Bình Trưng Tây", "Phường Cát Lái", "Phường Thảo Điền", "Phường Thạnh Mỹ Lợi", "Phường Thủ Thiêm" } },
                { "Quận 3", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14" } },
                { "Quận 4", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 8", "Phường 9", "Phường 10", "Phường 13", "Phường 14", "Phường 15", "Phường 16", "Phường 18" } },
                { "Quận 5", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14" } },
                { "Quận 6", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14" } },
                { "Quận 7", new List<string> { "Phường Bình Thuận", "Phường Phú Mỹ", "Phường Phú Thuận", "Phường Tân Hưng", "Phường Tân Kiểng", "Phường Tân Phong", "Phường Tân Phú", "Phường Tân Quy", "Phường Tân Thuận Đông", "Phường Tân Thuận Tây" } },
                { "Quận 8", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16" } },
                { "Quận 9", new List<string> { "Phường Hiệp Phú", "Phường Long Bình", "Phường Long Phước", "Phường Long Thạnh Mỹ", "Phường Long Trường", "Phường Phú Hữu", "Phường Phước Bình", "Phường Phước Long A", "Phường Phước Long B", "Phường Tân Phú", "Phường Tăng Nhơn Phú A", "Phường Tăng Nhơn Phú B", "Phường Trường Thạnh" } },
                { "Quận 10", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15" } },
                { "Quận 11", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16" } },
                { "Quận 12", new List<string> { "Phường An Phú Đông", "Phường Đông Hưng Thuận", "Phường Hiệp Thành", "Phường Tân Chánh Hiệp", "Phường Tân Hưng Thuận", "Phường Tân Thới Hiệp", "Phường Tân Thới Nhất", "Phường Thạnh Lộc", "Phường Thạnh Xuân", "Phường Thới An", "Phường Trung Mỹ Tây" } },
                { "Quận Bình Tân", new List<string> { "Phường An Lạc", "Phường An Lạc A", "Phường Bình Hưng Hòa", "Phường Bình Hưng Hòa A", "Phường Bình Hưng Hòa B", "Phường Bình Trị Đông", "Phường Bình Trị Đông A", "Phường Bình Trị Đông B", "Phường Tân Tạo", "Phường Tân Tạo A" } },
                { "Quận Bình Thạnh", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 5", "Phường 6", "Phường 7", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 17", "Phường 19", "Phường 21", "Phường 22", "Phường 24", "Phường 25", "Phường 26", "Phường 27", "Phường 28" } },
                { "Quận Gò Vấp", new List<string> { "Phường 1", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16", "Phường 17" } },
                { "Quận Phú Nhuận", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 13", "Phường 15", "Phường 17" } },
                { "Quận Tân Bình", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15" } },
                { "Quận Tân Phú", new List<string> { "Phường Hiệp Tân", "Phường Hòa Thạnh", "Phường Phú Thạnh", "Phường Phú Thọ Hòa", "Phường Phú Trung", "Phường Sơn Kỳ", "Phường Tân Quý", "Phường Tân Sơn Nhì", "Phường Tân Thành", "Phường Tây Thạnh", "Phường Trung Mỹ Tây" } },
                { "Quận Thủ Đức", new List<string> { "Phường Bình Chiểu", "Phường Bình Thọ", "Phường Hiệp Bình Chánh", "Phường Hiệp Bình Phước", "Phường Linh Chiểu", "Phường Linh Đông", "Phường Linh Tây", "Phường Linh Trung", "Phường Linh Xuân", "Phường Tam Bình", "Phường Tam Phú", "Phường Trường Thọ" } },
                { "Huyện Bình Chánh", new List<string> { "Xã An Phú Tây", "Xã Bình Chánh", "Xã Bình Hưng", "Xã Bình Lợi", "Xã Đa Phước", "Xã Hưng Long", "Xã Lê Minh Xuân", "Xã Phong Phú", "Xã Quy Đức", "Xã Tân Kiên", "Xã Tân Nhựt", "Xã Tân Quý Tây", "Xã Vĩnh Lộc A", "Xã Vĩnh Lộc B" } },
                { "Huyện Cần Giờ", new List<string> { "Thị trấn Cần Thạnh", "Xã An Thới Đông", "Xã Bình Khánh", "Xã Long Hòa", "Xã Lý Nhơn", "Xã Tam Thôn Hiệp", "Xã Thạnh An" } },
                { "Huyện Củ Chi", new List<string> { "Thị trấn Củ Chi", "Xã An Nhơn Tây", "Xã An Phú", "Xã Bình Mỹ", "Xã Hòa Phú", "Xã Nhuận Đức", "Xã Phạm Văn Cội", "Xã Phú Hòa Đông", "Xã Phú Mỹ Hưng", "Xã Phước Hiệp", "Xã Phước Thạnh", "Xã Phước Vĩnh An", "Xã Tân An Hội", "Xã Tân Phú Trung", "Xã Tân Thạnh Đông", "Xã Tân Thạnh Tây", "Xã Tân Thông Hội", "Xã Thái Mỹ", "Xã Trung An", "Xã Trung Lập Hạ", "Xã Trung Lập Thượng" } },
                { "Huyện Hóc Môn", new List<string> { "Thị trấn Hóc Môn", "Xã Bà Điểm", "Xã Đông Thạnh", "Xã Nhị Bình", "Xã Tân Hiệp", "Xã Tân Thới Nhì", "Xã Tân Xuân", "Xã Thới Tam Thôn", "Xã Trung Chánh", "Xã Xuân Thới Đông", "Xã Xuân Thới Sơn", "Xã Xuân Thới Thượng" } },
                { "Huyện Nhà Bè", new List<string> { "Thị trấn Nhà Bè", "Xã Hiệp Phước", "Xã Long Thới", "Xã Nhơn Đức", "Xã Phú Xuân", "Xã Phước Kiển", "Xã Phước Lộc" } }
            };
        }
    }
}