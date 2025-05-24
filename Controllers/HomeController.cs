using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebBanMayTinh.Models;
using WebBanMayTinh.Repositories;
using WebBanMayTinh.Data;
using System.Threading.Tasks;
using System.Linq;

namespace WebBanMayTinh.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository; // Thêm repository cho danh mục

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository; // Inject CategoryRepository
        }

        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            // Lấy danh sách danh mục
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories;

            // Lấy danh sách sản phẩm
            const int pageSize = 8;
            var products = await _productRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                products = products.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(searchQuery)) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(searchQuery))
                ).ToList();
            }

            var totalItems = products.Count();
            products = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SearchQuery = searchQuery;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> FilterProducts(string searchQuery, int page = 1)
        {
            const int pageSize = 8;
            var products = await _productRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                products = products.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(searchQuery)) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(searchQuery))
                ).ToList();
            }

            var totalItems = products.Count();
            products = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var summary = new
            {
                SearchQuery = searchQuery,
                TotalProducts = totalItems,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            var productListHtml = await this.RenderViewToStringAsync("_ProductList", products);

            return Json(new
            {
                productList = productListHtml,
                summary = summary
            });
        }

        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> GetSearchSuggestions(string term)
        {
            var products = await _productRepository.GetAllAsync();
            var suggestions = products
                .Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(term.ToLower())) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(term.ToLower()))
                )
                .Select(p => p.Name)
                .Distinct()
                .Take(10)
                .ToList();

            return Json(suggestions);
        }

        public IActionResult TechNews()
        {
            return View();
        }

        public IActionResult RepairService()
        {
            return View();
        }

        public IActionResult HomeTechnicalService()
        {
            return View();
        }

        public IActionResult TradeIn()
        {
            return View();
        }

        public IActionResult WarrantyCheck()
        {
            return View();
        }

        [HttpGet]
        public IActionResult BuildPcPromotion()
        {
            var components = ComponentData.GetComponents();
            ViewBag.Components = components;
            return View(new PcBuild());
        }

        [HttpPost]
        public IActionResult BuildPcPromotion(int? cpuId, int? gpuId, int? ramId, int? storageId, int? mainboardId, int? psuId, int? caseId, int? coolingId)
        {
            var components = ComponentData.GetComponents();
            var pcBuild = new PcBuild
            {
                Cpu = cpuId.HasValue ? components.FirstOrDefault(c => c.Id == cpuId.Value && c.Type == "CPU") : null,
                Gpu = gpuId.HasValue ? components.FirstOrDefault(c => c.Id == gpuId.Value && c.Type == "GPU") : null,
                Ram = ramId.HasValue ? components.FirstOrDefault(c => c.Id == ramId.Value && c.Type == "RAM") : null,
                Storage = storageId.HasValue ? components.FirstOrDefault(c => c.Id == storageId.Value && c.Type == "Storage") : null,
                Mainboard = mainboardId.HasValue ? components.FirstOrDefault(c => c.Id == mainboardId.Value && c.Type == "Mainboard") : null,
                Psu = psuId.HasValue ? components.FirstOrDefault(c => c.Id == psuId.Value && c.Type == "PSU") : null,
                Case = caseId.HasValue ? components.FirstOrDefault(c => c.Id == caseId.Value && c.Type == "Case") : null,
                Cooling = coolingId.HasValue ? components.FirstOrDefault(c => c.Id == coolingId.Value && c.Type == "Cooling") : null
            };

            ViewBag.Components = components;
            return View(pcBuild);
        }
    }
}