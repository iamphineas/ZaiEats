using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using ZaiEats.Data;
using ZaiEats.Models;

namespace ZaiEats.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RestaurantController(IWebHostEnvironment hostEnvironment, ApplicationDbContext context)
        {
            _hostEnvironment = hostEnvironment;
            _context = context;
        }

        // GET: Restaurant/CreateRestaurant
        public IActionResult CreateRestaurant()
        {
            return View();
        }

        // POST: Restaurant/CreateRestaurant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRestaurant(Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                if (restaurant.ImageFile != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string imagesPath = Path.Combine(wwwRootPath, "images");

                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(restaurant.ImageFile.FileName);
                    string filePath = Path.Combine(imagesPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await restaurant.ImageFile.CopyToAsync(stream);
                    }

                    restaurant.ImageUrl = "/images/" + fileName;
                }

                _context.Add(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            return View(restaurant);
        }

        public IActionResult ViewMenu(int id)
        {
            var restaurant = _context.Restaurants
                .Where(r => r.RestaurantId == id)
                .Select(r => new
                {
                    r.RestaurantId,
                    r.Name,
                    r.Description,
                    r.ImageUrl,
                    Categories = _context.MenuCategories
                        .Where(c => c.RestaurantId == r.RestaurantId)
                        .Select(c => new
                        {
                            c.Name,
                            MenuItems = _context.MenuItems
                                .Where(m => m.MenuCategoryId == c.MenuCategoryId)
                                .Select(m => new
                                {
                                    m.MenuItemId,
                                    m.Name,
                                    m.Description,
                                    m.Price,
                                    m.ImageUrl,
                                    Options = _context.MenuOptionGroups
                                        .Where(g => g.MenuItemId == m.MenuItemId)
                                        .Select(g => new
                                        {
                                            g.GroupName,
                                            g.IsRequired,
                                            OptionItems = _context.MenuOptionItems
                                                .Where(oi => oi.MenuOptionGroupId == g.MenuOptionGroupId)
                                                .ToList()
                                        }).ToList()
                                }).ToList()
                        }).ToList()
                }).FirstOrDefault();

            if (restaurant == null)
            {
                return NotFound(); // prevents view rendering with null model
            }

            return View(restaurant);
        }



    }
}
