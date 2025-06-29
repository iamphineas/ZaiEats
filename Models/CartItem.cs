namespace ZaiEats.Models
{
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = "";
        public List<CartOptionSelection> Options { get; set; } = new();

        // price per unit including options
        public decimal UnitPrice => Price + Options.Sum(o => o.AdditionalPrice);

        public decimal LineTotal => UnitPrice * Quantity;
    }


    public class CartOptionSelection
    {
        public int MenuOptionGroupId { get; set; }
        public int MenuOptionItemId { get; set; }
        public string OptionName { get; set; } = "";
        public decimal AdditionalPrice { get; set; }
    }
}
