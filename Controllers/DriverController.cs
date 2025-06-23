using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZaiEats.Data;
using ZaiEats.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ZaiEats.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context; private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DriverController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult CreateDriver()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDriver(Driver driver)
        {
            if (ModelState.IsValid)
            {
                // Save Driver record
                _context.Add(driver);
                await _context.SaveChangesAsync();

                // Create Identity user
                var user = new ApplicationUser
                {
                    UserName = driver.Email,
                    Email = driver.Email,
                    FullName = driver.FullName
                };

                var result = await _userManager.CreateAsync(user, "Test123!"); // Default password

                if (result.Succeeded)
                {
                    // Ensure "Driver" role exists
                    if (!await _roleManager.RoleExistsAsync("Driver"))
                        await _roleManager.CreateAsync(new IdentityRole("Driver"));

                    await _userManager.AddToRoleAsync(user, "Driver");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create driver login.");
                }

                return RedirectToAction("Index", "Home");
            }

            return View(driver);
        }


    }

}
