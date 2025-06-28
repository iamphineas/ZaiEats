using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZaiEats.Data;
using ZaiEats.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ZaiEats.Controllers
{
    public class KitchenStaffController : Controller
    {
        private readonly ApplicationDbContext _context; private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public KitchenStaffController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "KitchenStaff")]
        public IActionResult IncomingOrders()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateKitchenStaff()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateKitchenStaff(KitchenStaff kitchenStaff)
        {
            if (ModelState.IsValid)
            {
                // Save Driver record
                _context.Add(kitchenStaff);
                await _context.SaveChangesAsync();

                // Create Identity user
                var user = new ApplicationUser
                {
                    UserName = kitchenStaff.Email,
                    Email = kitchenStaff.Email,
                    FullName = kitchenStaff.FullName
                };

                var result = await _userManager.CreateAsync(user, "Test123!"); // Default password

                if (result.Succeeded)
                {
                    // Ensure "Driver" role exists
                    if (!await _roleManager.RoleExistsAsync("KitchenStaff"))
                        await _roleManager.CreateAsync(new IdentityRole("KitchenStaff"));

                    await _userManager.AddToRoleAsync(user, "KitchenStaff");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create kitchen staff login.");
                }

                return RedirectToAction("Index", "Home");
            }

            return View(kitchenStaff);
        }

    }
}
