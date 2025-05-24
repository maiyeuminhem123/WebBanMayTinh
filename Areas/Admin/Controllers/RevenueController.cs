namespace WebBanMayTinh.Areas.Admin.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using WebBanMayTinh.Models;
    using WebBanMayTinh.Services;
    using Microsoft.AspNetCore.Mvc;
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    
    public class RevenueController : Controller
    {
        private readonly IRevenueService _revenueService;

        public RevenueController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        /// <summary>
        /// Dashboard Razor page (GET) – render với ViewModel.
        /// </summary>
        public async Task<IActionResult> Dashboard(string reportType = "day", DateTime? startDate = null, DateTime? endDate = null, string category = "all")
        {
            var from = startDate ?? DateTime.Today.AddMonths(-1);
            var to = endDate ?? DateTime.Today;

            var chartData = await _revenueService.GetRevenueDataAsync(reportType, from, to, category);
            var categoryData = await _revenueService.GetCategoryDistributionAsync(from, to, category);

            var summary = new RevenueSummary
            {
                TotalRevenue = chartData.Sum(x => x.Revenue),
                TotalOrders = chartData.Sum(x => x.Orders),
                AverageOrderValue = chartData.Sum(x => x.Revenue) / Math.Max(1, chartData.Sum(x => x.Orders)),
                GrowthRate = chartData.LastOrDefault()?.Growth ?? 0
            };

            var vm = new RevenueDashboardViewModel
            {
                ReportType = reportType,
                StartDate = from,
                EndDate = to,
                Category = category,
                ChartData = chartData,
                CategoryData = categoryData,
                Summary = summary
            };

            return View(vm);
        }

        /// <summary>
        /// Ajax JSON – trả về dữ liệu (biểu đồ + pie + summary)
        /// POST /Revenue/Data
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Data([FromBody] RevenueQuery query)
        {
            var chartData = await _revenueService.GetRevenueDataAsync(query.ReportType, query.StartDate, query.EndDate, query.Category);
            var categoryData = await _revenueService.GetCategoryDistributionAsync(query.StartDate, query.EndDate, query.Category);
            var summary = new RevenueSummary
            {
                TotalRevenue = chartData.Sum(x => x.Revenue),
                TotalOrders = chartData.Sum(x => x.Orders),
                AverageOrderValue = chartData.Sum(x => x.Revenue) / Math.Max(1, chartData.Sum(x => x.Orders)),
                GrowthRate = chartData.LastOrDefault()?.Growth ?? 0
            };
            return Json(new { chartData, categoryData, summary });
        }
    }
}