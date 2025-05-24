using WebBanMayTinh.Models;

namespace WebBanMayTinh.Repositories
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<Review> GetByIdAsync(int id);
        Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(int id);
    }
}
