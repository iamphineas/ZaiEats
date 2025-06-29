using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZaiEats.Data;
using ZaiEats.Models;
using ZaiEats.Services;
using ZaiEats.ViewModels;

namespace ZaiEats.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly Cart _cart;

        public CartController(ApplicationDbContext db, Cart cart)
        {
            _db = db;
            _cart = cart;
        }

        // GET /Cart
        public IActionResult Index()
        {
            var vm = new CartViewModel
            {
                CartItems = (List<CartItem>)_cart.GetItems()
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult AddToCart([FromBody] AddToCartRequest req)
        {
            var menuItem = _db.MenuItems
                .Include(mi => mi.OptionGroups)
                  .ThenInclude(g => g.OptionItems)
                .FirstOrDefault(mi => mi.MenuItemId == req.MenuItemId);
            if (menuItem == null) return NotFound();

            var opts = new List<CartOptionSelection>();
            if (req.OptionItemIds != null)
            {
                foreach (var optId in req.OptionItemIds)
                {
                    var opt = _db.MenuOptionItems.Find(optId);
                    if (opt != null)
                        opts.Add(new CartOptionSelection
                        {
                            MenuOptionGroupId = opt.MenuOptionGroupId,
                            MenuOptionItemId = opt.MenuOptionItemId,
                            OptionName = opt.OptionName,
                            AdditionalPrice = opt.AdditionalPrice ?? 0m
                        });
                }
            }

            _cart.AddItem(menuItem, req.Qty, opts);
            return Json(new
            {
                success = true,
                totalItems = _cart.TotalItems()
            });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int menuItemId)
        {
            _cart.RemoveItem(menuItemId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            _cart.Clear();
            return Json(new { success = true });
        }
    }
}
