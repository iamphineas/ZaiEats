using System.Collections.Generic;
using System.Linq;
using ZaiEats.Models;

namespace ZaiEats.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; } = new();

        /// <summary>
        /// Sum of each line‐total (price + options) * quantity
        /// </summary>
        public decimal Subtotal => CartItems.Sum(i => i.LineTotal);

        /// <summary>
        /// Flat delivery fee for now
        /// </summary>
        public decimal DeliveryFee { get; set; } = 25m;

        public decimal Total => Subtotal + DeliveryFee;
    }
}
