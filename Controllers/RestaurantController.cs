using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
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

        public IActionResult Index()
        {
            var restaurants = _context.Restaurants.ToList();
            return View(restaurants);
        }

        public IActionResult CreateRestaurant()
        {
            return View();
        }

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
                return RedirectToAction("Index");
            }

            return View(restaurant);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();
            return View(restaurant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Restaurant restaurant)
        {
            if (id != restaurant.RestaurantId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (restaurant.ImageFile != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(restaurant.ImageFile.FileName);
                        string path = Path.Combine(wwwRootPath, "images", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await restaurant.ImageFile.CopyToAsync(stream);
                        }

                        restaurant.ImageUrl = "/images/" + fileName;
                    }

                    _context.Update(restaurant);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Restaurants.Any(e => e.RestaurantId == id))
                        return NotFound();
                    throw;
                }
            }
            return View(restaurant);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();
            return View(restaurant);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant != null)
            {
                _context.Restaurants.Remove(restaurant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
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
                return NotFound();
            }

            return View(restaurant);
        }
    }
}
