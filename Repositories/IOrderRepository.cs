using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(int id);               
        Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
        Task DeleteRangeAsync(IEnumerable<int> ids);
        Task DeleteAsync(Order order);

    }
}