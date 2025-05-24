using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.StatusHistory)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .Include(o => o.StatusHistory)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<int> ids)
        {
            var orders = await _context.Orders
                .Where(o => ids.Contains(o.Id))
                .ToListAsync();
            if (orders.Any())
            {
                _context.Orders.RemoveRange(orders);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAsync(Order order)
        {
            // Gán thời gian hiện tại
            order.OrderDate = DateTime.Now;

            // Thêm Order vào context
            _context.Orders.Add(order);

            // Lưu vào database để sinh Id
            await _context.SaveChangesAsync();

            // Gán OrderCode sau khi có Id
            order.OrderCode = $"ORD-{order.Id:D6}";

            // Cập nhật lại Order với OrderCode mới
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Order order)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}