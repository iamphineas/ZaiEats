using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZaiEats.Data;
using ZaiEats.Models;

namespace ZaiEats.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            return View();
        }

        // GET: /Customer/Orders
        public IActionResult Orders()
        {
            return View();
        }
    }
}
