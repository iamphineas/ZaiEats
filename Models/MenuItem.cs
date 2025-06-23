using System.ComponentModel.DataAnnotations.Schema;

namespace ZaiEats.Models
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }

        public int MenuCategoryId { get; set; }
        public MenuCategory MenuCategory { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
        public bool IsAvailable { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }

        public ICollection<MenuOptionGroup> OptionGroups { get; set; }
    }
}
