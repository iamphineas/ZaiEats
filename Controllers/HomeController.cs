using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ZaiEats.Data;
using ZaiEats.Models;

namespace ZaiEats.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var restaurants = _context.Restaurants.ToList();
            return View(restaurants);
        }
    }
}
