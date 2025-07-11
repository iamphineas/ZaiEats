using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ZaiEats.Data;
using ZaiEats.Models;
using ZaiEats.ViewModels;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity;


namespace ZaiEats.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        // GET: Admin/CreateMenuItem
        public IActionResult CreateMenuItem()
        {
            var vm = new MenuItemViewModel
            {
                RestaurantList = _context.Restaurants
                    .Select(r => new SelectListItem { Value = r.RestaurantId.ToString(), Text = r.Name }).ToList(),

                CategoryList = _context.MenuCategories
                    .Select(c => new SelectListItem { Value = c.MenuCategoryId.ToString(), Text = c.Name }).ToList()
            };

            return View(vm);
        }

        // POST: Admin/CreateMenuItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenuItem(MenuItemViewModel vm)
        {
            // Prevent validation on view-only dropdowns
            ModelState.Remove("RestaurantList");
            ModelState.Remove("CategoryList");

            if (!ModelState.IsValid)
            {
                vm.RestaurantList = _context.Restaurants
                    .Select(r => new SelectListItem { Value = r.RestaurantId.ToString(), Text = r.Name }).ToList();
                vm.CategoryList = _context.MenuCategories
                    .Select(c => new SelectListItem { Value = c.MenuCategoryId.ToString(), Text = c.Name }).ToList();
                return View(vm);
            }

            // Image upload logic using WebRootPath (same as Restaurant upload)
            string imageUrl = "";
            if (vm.ImageFile != null)
            {
                string wwwRootPath = _env.WebRootPath;
                string imagesPath = Path.Combine(wwwRootPath, "images");

                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.ImageFile.FileName);
                string filePath = Path.Combine(imagesPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }

                imageUrl = "/images/" + fileName;
            }

            // Create MenuItem
            var item = new MenuItem
            {
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                ImageUrl = imageUrl,
                MenuCategoryId = vm.MenuCategoryId,
                RestaurantId = vm.RestaurantId,
                IsAvailable = true
            };

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            // Add Option Groups & Items
            if (vm.OptionGroups != null)
            {
                foreach (var group in vm.OptionGroups)
                {
                    var optionGroup = new MenuOptionGroup
                    {
                        MenuItemId = item.MenuItemId,
                        GroupName = group.GroupName,
                        IsRequired = group.IsRequired
                    };
                    _context.MenuOptionGroups.Add(optionGroup);
                    await _context.SaveChangesAsync();

                    if (group.OptionItems != null)
                    {
                        foreach (var opt in group.OptionItems)
                        {
                            var optionItem = new MenuOptionItem
                            {
                                MenuOptionGroupId = optionGroup.MenuOptionGroupId,
                                OptionName = opt.OptionName,
                                AdditionalPrice = opt.AdditionalPrice
                            };
                            _context.MenuOptionItems.Add(optionItem);
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }
        public IActionResult CreateMenuCategory()
        {
            ViewBag.Restaurants = new SelectList(_context.Restaurants, "RestaurantId", "Name");
            return View();
        }

        [HttpGet]
        public JsonResult GetCategoriesByRestaurant(int restaurantId)
        {
            var categories = _context.MenuCategories
                .Where(c => c.RestaurantId == restaurantId)
                .Select(c => new { c.MenuCategoryId, c.Name })
                .ToList();

            return Json(categories);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenuCategory(MenuCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.MenuCategories.Add(category);
                await _context.SaveChangesAsync();

                TempData["ShowCategorySuccess"] = true;
                TempData["SavedCategoryName"] = category.Name;

                return RedirectToAction("CreateMenuCategory");
            }

            ViewBag.Restaurants = new SelectList(_context.Restaurants, "RestaurantId", "Name", category.RestaurantId);
            return View(category);
        }


        // GET: Admin/AssignManagers/5
        public async Task<IActionResult> AssignManagers(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null)
                return NotFound();

            var allManagers = await _userManager.GetUsersInRoleAsync("Manager");
            var assignedIds = _context.RestaurantManagers
                .Where(rm => rm.RestaurantId == restaurantId)
                .Select(rm => rm.ManagerId)
                .ToList();

            var model = new AssignManagerViewModel
            {
                RestaurantId = restaurantId,
                RestaurantName = restaurant.Name,
                AllManagers = allManagers.Select(m => new ManagerCheckbox
                {
                    ManagerId = m.Id,
                    FullName = m.FullName,
                    IsAssigned = assignedIds.Contains(m.Id)
                }).ToList()
            };

            return View(model);
        }

        // POST: Admin/AssignManagers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignManagers(AssignManagerViewModel model)
        {
            // Remove existing assignments
            var toRemove = _context.RestaurantManagers.Where(r => r.RestaurantId == model.RestaurantId);
            _context.RestaurantManagers.RemoveRange(toRemove);
            await _context.SaveChangesAsync();

            // Add selected ones
            foreach (var manager in model.AllManagers.Where(m => m.IsAssigned))
            {
                _context.RestaurantManagers.Add(new RestaurantManager
                {
                    RestaurantId = model.RestaurantId,
                    ManagerId = manager.ManagerId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Restaurant");
        }

        // Optional: Admin action to remove manager from all restaurants
        [HttpPost]
        public async Task<IActionResult> RemoveManagerFromAll(string managerId)
        {
            var links = _context.RestaurantManagers.Where(rm => rm.ManagerId == managerId);
            _context.RestaurantManagers.RemoveRange(links);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public IActionResult CreateManager()
        {
            var model = new CreateManagerViewModel
            {
                Restaurants = _context.Restaurants
                    .Select(r => new SelectListItem
                    {
                        Value = r.RestaurantId.ToString(),
                        Text = r.Name
                    }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManager(CreateManagerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Restaurants = _context.Restaurants
                    .Select(r => new SelectListItem
                    {
                        Value = r.RestaurantId.ToString(),
                        Text = r.Name
                    }).ToList();
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Manager");

                if (model.SelectedRestaurantIds != null)
                {
                    foreach (var restId in model.SelectedRestaurantIds)
                    {
                        _context.RestaurantManagers.Add(new RestaurantManager
                        {
                            RestaurantId = restId,
                            ManagerId = user.Id
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Manager created successfully!";
                return RedirectToAction("CreateManager");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            model.Restaurants = _context.Restaurants
                .Select(r => new SelectListItem
                {
                    Value = r.RestaurantId.ToString(),
                    Text = r.Name
                }).ToList();

            return View(model);
        }


        public async Task<IActionResult> ListManagers()
        {
            var managers = await _userManager.GetUsersInRoleAsync("Manager");

            var model = managers.Select(m => new ManagerListViewModel
            {
                ManagerId = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                AssignedRestaurants = _context.RestaurantManagers
                    .Where(rm => rm.ManagerId == m.Id)
                    .Select(rm => rm.Restaurant.Name)
                    .ToList()
            }).ToList();

            return View(model);
        }
    }
}