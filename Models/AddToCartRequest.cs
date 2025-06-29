namespace ZaiEats.Models
{
    public class AddToCartRequest
    {
        public int MenuItemId { get; set; }
        public int Qty { get; set; }
        public List<int>? OptionItemIds { get; set; }
    }
}
