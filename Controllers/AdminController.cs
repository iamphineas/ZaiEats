using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ZaiEats.Data;
using ZaiEats.Models;
using ZaiEats.ViewModels;
using System.Globalization;
using Microsoft.AspNetCore.Localization;


namespace ZaiEats.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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



    }
}
