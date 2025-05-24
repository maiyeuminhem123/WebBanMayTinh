namespace WebBanMayTinh.Services
{
    using Microsoft.EntityFrameworkCore;
    using WebBanMayTinh.Models;
    

    public interface IRevenueService
    {
        Task<IReadOnlyList<RevenueData>> GetRevenueDataAsync(string reportType, DateTime start, DateTime end, string category);
        Task<IReadOnlyList<CategoryDistribution>> GetCategoryDistributionAsync(DateTime start, DateTime end, string category);
    }

    public class RevenueService : IRevenueService
    {
        private readonly ApplicationDbContext _context;

        public RevenueService(ApplicationDbContext context)
        {
            _context = context;
        }

        /* --------------------------------------
         * Trả danh sách RevenueData theo thời gian
         * --------------------------------------*/
        public async Task<IReadOnlyList<RevenueData>> GetRevenueDataAsync(string reportType, DateTime start, DateTime end, string category)
        {
            // Lấy dữ liệu OrderDetails có Order trong khoảng thời gian lọc
            var query = _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Where(od => od.Order.OrderDate >= start && od.Order.OrderDate <= end);

            // Nếu filter theo category cụ thể
            if (!string.IsNullOrWhiteSpace(category) && category != "all")
            {
                query = query.Where(od => od.Product.Category!.Name == category);
            }

            // Nhóm theo ngày/tháng/năm tùy reportType (group key là DateTime)
            var groupedQuery = reportType switch
            {
                "month" => query.GroupBy(od => new DateTime(od.Order.OrderDate.Year, od.Order.OrderDate.Month, 1)),
                "year" => query.GroupBy(od => new DateTime(od.Order.OrderDate.Year, 1, 1)),
                _ => query.GroupBy(od => od.Order.OrderDate.Date)
            };

            // Lấy dữ liệu đã nhóm từ DB, chưa convert DateTime thành string
             
var groupedData = await groupedQuery  
    .Select(g => new  
    {  
        Date = g.Key,  
        Revenue = g.Sum(od => od.Quantity * od.UnitPrice),  
        Orders = g.Select(od => od.OrderId).Distinct().Count()  
    })  
    .OrderBy(g => g.Date)  
    .ToListAsync();  

var result = groupedData  
    .Select(g => new RevenueData  
    {  
        Date = g.Date.ToString("yyyy-MM-dd"),  
        Revenue = g.Revenue,  
        Orders = g.Orders,  
        Average = g.Orders == 0 ? 0 : g.Revenue / g.Orders,  
        Growth = 0  
    })  
    .ToList();  


            // Tính Growth % giữa các bản ghi liên tiếp
            for (int i = 1; i < result.Count; i++)
            {
                var prev = result[i - 1].Revenue;
                var curr = result[i].Revenue;
                result[i].Growth = prev == 0 ? 0 : Math.Round((double)(curr - prev) / (double)prev * 100, 2);
            }

            return result;
        }



        /* ------------------------------------------------
         * Phân bố doanh thu theo danh mục cho pie chart
         * ------------------------------------------------*/
        public async Task<IReadOnlyList<CategoryDistribution>> GetCategoryDistributionAsync(DateTime start, DateTime end, string category)
        {
            // Nếu đã lọc 1 category cụ thể, không cần pie chart -> trả empty list
            if (!string.IsNullOrWhiteSpace(category) && category != "all")
                return new List<CategoryDistribution>();

            var data = await _context.OrderDetails
                           .Include(od => od.Product)
                           .Include(od => od.Order)
                           .Where(od => od.Order != null && od.Order.OrderDate >= start && od.Order.OrderDate <= end)
                           .GroupBy(od => od.Product.Category!.Name)
                           .Select(g => new { Category = g.Key, Revenue = g.Sum(x => x.Quantity * x.UnitPrice) })
                           .ToListAsync();


            var total = data.Sum(x => x.Revenue);
            return data.Select(x => new CategoryDistribution
            {
                Category = x.Category,
                Revenue = x.Revenue,
                Percentage = total == 0 ? 0 : Math.Round((double)(x.Revenue / total) * 100d, 2)
            }).ToList();
        }
    }
}