using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Repositories
{
    public interface ICategoryService
    {
        Task<List<Category>> GetCategoriesAsync();
    }

}
