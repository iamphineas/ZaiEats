using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZaiEats.Data;
using ZaiEats.Models;
using ZaiEats.Services;
using ZaiEats.ViewModels;

namespace ZaiEats.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Cart _cart;

        public CustomerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, Cart cart)
        {
            _context = context;
            _userManager = userManager;
            _cart = cart;
        }

        // GET: /Customer/Profile
        public async Task<IActionResult> Profile()
        {
            //var user = await _userManager.GetUserAsync(User);
            //if (user == null)
            //{
            //    return RedirectToAction("Login", "Account");
            //}

            //return View(user); // Passes ApplicationUser model to the view
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var fullName = user.FullName ?? "";
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = parts.FirstOrDefault() ?? "";
            var lastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "";

            ViewBag.FirstName = firstName;
            ViewBag.LastName = lastName;

            return View(user);
        }

        // GET: /Customer/Cart
        public IActionResult Cart()
        {
            var vm = new CartViewModel
            {
                CartItems = _cart.GetItems().ToList(),   // or cast to List<CartItem>
                                                         // DeliveryFee etc. come from VM defaults
            };
            return View(vm);
        }

        // GET: /Customer/Orders
        public IActionResult Orders()
        {
            return View();
        }
    }
}
