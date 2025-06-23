using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ZaiEats.Models
{
    public class MenuCategory
    {
        public int MenuCategoryId { get; set; }

        [Required]
        public int RestaurantId { get; set; }
        [ValidateNever]
        public Restaurant Restaurant { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }
        [ValidateNever]
        public ICollection<MenuItem> MenuItems { get; set; }
    }
}
