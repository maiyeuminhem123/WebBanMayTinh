using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebBanMayTinh.Extensions;
using WebBanMayTinh.Models;
using WebBanMayTinh.Repositories;

namespace WebBanMayTinh.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PayPalOptions _payPalOptions;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IOptions<PayPalOptions> payPalOptions,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            ILogger<PaymentController> logger)
        {
            _payPalOptions = payPalOptions?.Value ?? throw new ArgumentNullException(nameof(payPalOptions));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private PayPalHttpClient GetPayPalClient()
        {
            if (string.IsNullOrEmpty(_payPalOptions.ClientId) || string.IsNullOrEmpty(_payPalOptions.ClientSecret))
            {
                _logger.LogError("Cấu hình PayPal không hợp lệ: ClientId hoặc ClientSecret bị thiếu.");
                throw new InvalidOperationException("Cấu hình PayPal không hợp lệ.");
            }

            PayPalEnvironment environment;
            switch (_payPalOptions.Mode.ToLower())
            {
                case "sandbox":
                    environment = new SandboxEnvironment(_payPalOptions.ClientId, _payPalOptions.ClientSecret);
                    break;
                case "live":
                    environment = new LiveEnvironment(_payPalOptions.ClientId, _payPalOptions.ClientSecret);
                    break;
                default:
                    _logger.LogError("Chế độ PayPal không hợp lệ: {Mode}. Chỉ hỗ trợ 'sandbox' hoặc 'live'.", _payPalOptions.Mode);
                    throw new InvalidOperationException($"Chế độ PayPal không hợp lệ: {_payPalOptions.Mode}.");
            }

            return new PayPalHttpClient(environment);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (!cart.Items.Any())
            {
                _logger.LogWarning("Giỏ hàng trống khi tạo thanh toán PayPal.");
                return Json(new { success = false, message = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi thanh toán." });
            }

            decimal totalAmount = cart.Items.Sum(item => item.TotalPrice);
            if (totalAmount <= 0)
            {
                _logger.LogError("Tổng tiền không hợp lệ: {TotalAmount}", totalAmount);
                return Json(new { success = false, message = "Tổng tiền không hợp lệ." });
            }

            var items = cart.Items.Select(item => new Item
            {
                Name = item.Name,
                UnitAmount = new Money
                {
                    CurrencyCode = "USD",
                    Value = item.UnitPrice.ToString("F2", CultureInfo.InvariantCulture)
                },
                Quantity = item.Quantity.ToString(),
                Sku = item.ProductId.ToString()
            }).ToList();

            var client = GetPayPalClient();

            var orderRequest = new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        Description = "Thanh toán đơn hàng từ WebBanMayTinh",
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = "USD",
                            Value = totalAmount.ToString("F2", CultureInfo.InvariantCulture),
                            AmountBreakdown = new AmountBreakdown
                            {
                                ItemTotal = new Money
                                {
                                    CurrencyCode = "USD",
                                    Value = totalAmount.ToString("F2", CultureInfo.InvariantCulture)
                                }
                            }
                        },
                        Items = items
                    }
                },
                ApplicationContext = new ApplicationContext
                {
                    ReturnUrl = "http://localhost:5159/Payment/Success",
                    CancelUrl = "http://localhost:5159/Payment/Cancel"
                }
            };

            _logger.LogInformation("Tạo thanh toán PayPal. ReturnUrl: {ReturnUrl}, CancelUrl: {CancelUrl}",
                orderRequest.ApplicationContext.ReturnUrl, orderRequest.ApplicationContext.CancelUrl);

            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(orderRequest);

            try
            {
                var response = await client.Execute(request);
                var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                if (string.IsNullOrEmpty(result.Id))
                {
                    _logger.LogError("PayPal không trả về Order ID.");
                    return Json(new { success = false, message = "Không thể tạo thanh toán PayPal: Không nhận được Order ID." });
                }

                _logger.LogInformation("PayPal Order ID: {OrderId}", result.Id);
                HttpContext.Session.SetObjectAsJson("PayPalOrderId", result.Id);
                HttpContext.Session.SetObjectAsJson("PendingCart", cart);

                var approvalUrl = result.Links.FirstOrDefault(link => link.Rel.Equals("approve", StringComparison.OrdinalIgnoreCase))?.Href;
                if (approvalUrl != null)
                {
                    _logger.LogInformation("Tạo thanh toán thành công. OrderId: {OrderId}, ApprovalUrl: {ApprovalUrl}", result.Id, approvalUrl);
                    return Json(new { success = true, url = approvalUrl, orderId = result.Id });
                }

                _logger.LogError("Không tìm thấy approvalUrl từ PayPal.");
                return Json(new { success = false, message = "Không thể tạo thanh toán PayPal: Không tìm thấy URL phê duyệt." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tạo thanh toán PayPal: {Message}", ex.Message);
                return Json(new { success = false, message = $"Lỗi khi tạo thanh toán PayPal: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(string token, string paypalOrderId)
        {
            _logger.LogInformation("Chuyển hướng tới Success. Token: {Token}, PayPalOrderId: {PayPalOrderId}", token ?? "null", paypalOrderId ?? "null");

            // Lấy token từ tham số token hoặc paypalOrderId
            string orderId = token ?? paypalOrderId;

            // Nếu không có token từ query string, lấy từ session
            if (string.IsNullOrEmpty(orderId))
            {
                orderId = HttpContext.Session.GetObjectFromJson<string>("PayPalOrderId");
                _logger.LogInformation("Không tìm thấy token hoặc paypalOrderId trong query string. Lấy token từ session: {OrderId}", orderId ?? "null");
            }

            if (string.IsNullOrEmpty(orderId))
            {
                _logger.LogWarning("Không tìm thấy Order ID khi xử lý thanh toán thành công.");
                return View(new OrderViewModel { Message = "Thanh toán thất bại: Không tìm thấy Order ID." });
            }

            var client = GetPayPalClient();
            var request = new OrdersCaptureRequest(orderId);
            request.Prefer("return=representation");

            try
            {
                _logger.LogInformation("Gửi yêu cầu capture với Order ID: {OrderId}", orderId);
                var response = await client.Execute(request);
                var result = response.Result<PayPalCheckoutSdk.Orders.Order>();
                _logger.LogInformation("Kết quả từ PayPal: Status={Status}, OrderId={OrderId}", result.Status, result.Id);

                if (result.Status != "COMPLETED")
                {
                    _logger.LogWarning("PayPal order không hoàn tất. Status: {Status}", result.Status);
                    return View(new OrderViewModel { Message = "Thanh toán thất bại: Đơn hàng chưa được hoàn tất." });
                }

                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("PendingCart");
                var pendingOrder = HttpContext.Session.GetObjectFromJson<WebBanMayTinh.Models.Order>("PendingOrder");

                if (cart == null || !cart.Items.Any())
                {
                    _logger.LogWarning("Không tìm thấy giỏ hàng trong session khi xử lý thanh toán thành công.");
                    return View(new OrderViewModel { Message = "Thanh toán thất bại: Không tìm thấy thông tin giỏ hàng." });
                }

                if (pendingOrder == null)
                {
                    _logger.LogWarning("Không tìm thấy pending order trong session.");
                    return View(new OrderViewModel { Message = "Thanh toán thất bại: Không tìm thấy thông tin đơn hàng." });
                }

                // Cập nhật thông tin đơn hàng
                pendingOrder.Status = "Completed";
                pendingOrder.PaymentMethod = "PayPal";
                pendingOrder.TransactionId = orderId;
                pendingOrder.OrderDate = DateTime.Now;
                pendingOrder.OrderDetails = cart.Items.Select(item => new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList();

                _logger.LogInformation("Lưu đơn hàng vào cơ sở dữ liệu...");
                await _orderRepository.AddAsync(pendingOrder);

                // Xóa session sau khi thanh toán thành công
                HttpContext.Session.Remove("PayPalOrderId");
                HttpContext.Session.Remove("PendingCart");
                HttpContext.Session.Remove("PendingOrder");
                HttpContext.Session.Remove("Cart");

                // Lấy lại đơn hàng đã lưu (đảm bảo có OrderId và OrderDetails)
                var savedOrder = await _orderRepository.GetByIdAsync(pendingOrder.Id);
                if (savedOrder == null || savedOrder.OrderDetails == null)
                {
                    _logger.LogError("Không tìm thấy đơn hàng hoặc OrderDetails sau khi lưu.");
                    return View(new OrderViewModel { Message = "Thanh toán thất bại: Không thể truy xuất chi tiết đơn hàng." });
                }

                var orderViewModel = new OrderViewModel
                {
                    Message = "Thanh toán thành công qua PayPal!",
                    OrderId = savedOrder.Id,
                    TotalAmount = savedOrder.TotalAmount,
                    Items = savedOrder.OrderDetails.Select(od => new OrderItemViewModel
                    {
                        ProductId = od.ProductId,
                        Name = cart.Items.First(i => i.ProductId == od.ProductId).Name,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        TotalPrice = od.Quantity * od.UnitPrice
                    }).ToList()
                };

                _logger.LogInformation("Thanh toán PayPal thành công. OrderId: {OrderId}", savedOrder.Id);
                return View(orderViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xác nhận thanh toán PayPal: {Message}", ex.Message);
                return View(new OrderViewModel { Message = $"Thanh toán thất bại: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            _logger.LogInformation("Người dùng đã hủy thanh toán PayPal.");
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("PendingCart");
            var viewModel = new OrderViewModel
            {
                Message = "Thanh toán đã bị hủy.",
                Items = cart?.Items.Select(item => new OrderItemViewModel
                {
                    ProductId = item.ProductId,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                }).ToList() ?? new List<OrderItemViewModel>()
            };

            return View(viewModel);
        }
    }
}