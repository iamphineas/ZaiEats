namespace ZaiEats.Services
{
    // Services/Cart.cs
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using ZaiEats.Models;

    public class Cart
    {
        private const string SessionKey = "Cart";
        private readonly IHttpContextAccessor _ctx;
        public Cart(IHttpContextAccessor ctx) => _ctx = ctx;

        private List<CartItem> Items
        {
            get
            {
                var str = _ctx.HttpContext!.Session.GetString(SessionKey);
                return str == null
                    ? new List<CartItem>()
                    : JsonConvert.DeserializeObject<List<CartItem>>(str)!;
            }
            set
            {
                _ctx.HttpContext!.Session.SetString(SessionKey, JsonConvert.SerializeObject(value));
            }
        }

        public IReadOnlyList<CartItem> GetItems() => Items;

        public void AddItem(MenuItem menuItem, int qty, List<CartOptionSelection> opts)
        {
            // 1) grab the list once
            var items = Items;

            // 2) find an existing line (same item + same option combo)
            var existing = items.FirstOrDefault(ci =>
                ci.MenuItemId == menuItem.MenuItemId
                && ci.Options.Count == opts.Count
                && ci.Options.All(o => opts.Any(x => x.MenuOptionItemId == o.MenuOptionItemId))
            );

            if (existing != null)
            {
                existing.Quantity += qty;
            }
            else
            {
                items.Add(new CartItem
                {
                    MenuItemId = menuItem.MenuItemId,
                    Name = menuItem.Name,
                    Price = menuItem.Price,
                    Quantity = qty,
                    ImageUrl = menuItem.ImageUrl,
                    Options = opts
                });
            }

            // 3) now persist the mutated list
            Items = items;
        }


        public void RemoveItem(int menuItemId)
        {
            var toRemove = Items.Where(ci => ci.MenuItemId == menuItemId).ToList();
            foreach (var it in toRemove) Items.Remove(it);
            Items = Items;
        }

        public void Clear()
        {
            _ctx.HttpContext!.Session.Remove(SessionKey);
        }

        public int TotalItems() => Items.Sum(i => i.Quantity);

        public decimal SubTotal() => Items.Sum(i => i.LineTotal);
    }

}
