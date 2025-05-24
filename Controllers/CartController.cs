using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Extensions;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "ShoppingCart");
        }
    }
}
