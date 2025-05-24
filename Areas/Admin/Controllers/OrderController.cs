using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebBanMayTinh.Models;
using WebBanMayTinh.Repositories;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        // Hiển thị danh sách đơn hàng
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepository.GetAllAsync();
            return View(orders);
        }

        // Xem chi tiết đơn hàng
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // Xác nhận đơn hàng
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                if (order.Status != "Pending")
                {
                    return Json(new { success = false, message = "Đơn hàng không ở trạng thái chờ xử lý." });
                }

                order.Status = "Confirmed";
                order.StatusHistory ??= new List<OrderStatusHistory>();
                order.StatusHistory.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = "Confirmed",
                    UpdatedAt = DateTime.Now,
                    Notes = "Đơn hàng đã được xác nhận bởi Admin"
                });
                await _orderRepository.UpdateAsync(order);

                return Json(new { success = true, message = "Đơn hàng đã được xác nhận." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Xác nhận nhiều đơn hàng
        [HttpPost]
        public async Task<IActionResult> ConfirmSelected(int[] ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một đơn hàng." });
                }

                var orders = (await _orderRepository.GetAllAsync())
                    .Where(o => ids.Contains(o.Id) && o.Status == "Pending")
                    .ToList();

                if (!orders.Any())
                {
                    return Json(new { success = false, message = "Không có đơn hàng nào ở trạng thái chờ xử lý để xác nhận." });
                }

                foreach (var order in orders)
                {
                    order.Status = "Confirmed";
                    order.StatusHistory ??= new List<OrderStatusHistory>();
                    order.StatusHistory.Add(new OrderStatusHistory
                    {
                        OrderId = order.Id,
                        Status = "Confirmed",
                        UpdatedAt = DateTime.Now,
                        Notes = "Đơn hàng đã được xác nhận bởi Admin"
                    });
                }

                foreach (var order in orders)
                {
                    await _orderRepository.UpdateAsync(order);
                }

                return Json(new { success = true, message = "Đã xác nhận các đơn hàng được chọn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Hủy bỏ đơn hàng
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                order.Status = "Cancelled";
                order.StatusHistory ??= new List<OrderStatusHistory>();
                order.StatusHistory.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = "Cancelled",
                    UpdatedAt = DateTime.Now,
                    Notes = "Đơn hàng đã bị hủy bởi Admin"
                });
                await _orderRepository.UpdateAsync(order);

                return Json(new { success = true, message = "Đơn hàng đã bị hủy." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Hủy bỏ nhiều đơn hàng
        [HttpPost]
        public async Task<IActionResult> CancelSelected(int[] ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một đơn hàng." });
                }

                var orders = (await _orderRepository.GetAllAsync())
                    .Where(o => ids.Contains(o.Id))
                    .ToList();

                if (!orders.Any())
                {
                    return Json(new { success = false, message = "Không có đơn hàng nào để hủy." });
                }

                foreach (var order in orders)
                {
                    order.Status = "Cancelled";
                    order.StatusHistory ??= new List<OrderStatusHistory>();
                    order.StatusHistory.Add(new OrderStatusHistory
                    {
                        OrderId = order.Id,
                        Status = "Cancelled",
                        UpdatedAt = DateTime.Now,
                        Notes = "Đơn hàng đã bị hủy bởi Admin"
                    });
                    await _orderRepository.UpdateAsync(order);
                }

                return Json(new { success = true, message = "Đã hủy các đơn hàng được chọn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Chuyển trạng thái sang "Chuẩn bị hàng"
        [HttpPost]
        public async Task<IActionResult> Prepare(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại." });
                }
                Console.WriteLine($"Trạng thái đơn hàng {id}: {order.Status}");
                if (order.Status != "Confirmed")
                {
                    return Json(new { success = false, message = "Đơn hàng chưa được xác nhận." });
                }

                order.Status = "Preparing";
                order.StatusHistory ??= new List<OrderStatusHistory>();
                order.StatusHistory.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = "Preparing",
                    UpdatedAt = DateTime.Now,
                    Notes = "Đơn hàng đang được chuẩn bị"
                });
                await _orderRepository.UpdateAsync(order);

                return Json(new { success = true, message = "Đã chuyển trạng thái sang 'Chuẩn bị hàng'." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Chuyển trạng thái sang "Đang giao"
        [HttpPost]
        public async Task<IActionResult> Ship(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại." });
                }

                if (order.Status != "Preparing")
                {
                    return Json(new { success = false, message = "Đơn hàng chưa ở trạng thái chuẩn bị hàng." });
                }

                order.Status = "Shipping";
                order.StatusHistory ??= new List<OrderStatusHistory>();
                order.StatusHistory.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = "Shipping",
                    UpdatedAt = DateTime.Now,
                    Notes = "Đơn hàng đang được vận chuyển"
                });
                await _orderRepository.UpdateAsync(order);

                return Json(new { success = true, message = "Đã chuyển trạng thái sang 'Đang giao'." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Chuyển trạng thái sang "Đã giao thành công"
        [HttpPost]
        public async Task<IActionResult> Deliver(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại." });
                }

                if (order.Status != "Shipping")
                {
                    return Json(new { success = false, message = "Đơn hàng chưa ở trạng thái đang giao." });
                }

                order.Status = "Delivered";
                order.StatusHistory ??= new List<OrderStatusHistory>();
                order.StatusHistory.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = "Delivered",
                    UpdatedAt = DateTime.Now,
                    Notes = "Đơn hàng đã được giao đến khách"
                });
                await _orderRepository.UpdateAsync(order);

                return Json(new { success = true, message = "Đã chuyển trạng thái sang 'Đã giao thành công'." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
        // Xóa một đơn hàng
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                await _orderRepository.DeleteAsync(order);
                return Json(new { success = true, message = "Đã xóa đơn hàng thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Xóa nhiều đơn hàng
        [HttpPost]
        public async Task<IActionResult> DeleteSelected(int[] ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một đơn hàng để xóa." });
                }

                var orders = (await _orderRepository.GetAllAsync())
                    .Where(o => ids.Contains(o.Id))
                    .ToList();

                if (!orders.Any())
                {
                    return Json(new { success = false, message = "Không tìm thấy các đơn hàng cần xóa." });
                }

                foreach (var order in orders)
                {
                    await _orderRepository.DeleteAsync(order);
                }

                return Json(new { success = true, message = "Đã xóa các đơn hàng được chọn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

    }
}