using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZaiEats.ViewModels
{
    public class MenuItemViewModel
    {
        // Basic Menu Item Info
        public int RestaurantId { get; set; }
        public int MenuCategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        public IFormFile ImageFile { get; set; }

        // Dropdowns
        
        public List<SelectListItem> RestaurantList { get; set; }
        
        public List<SelectListItem> CategoryList { get; set; }

        // Option Groups with Option Items
        public List<MenuOptionGroupInput> OptionGroups { get; set; } = new();
    }

    public class MenuOptionGroupInput
    {
        public string GroupName { get; set; }
        public bool IsRequired { get; set; }
        public List<MenuOptionItemInput> OptionItems { get; set; } = new();
    }

    public class MenuOptionItemInput
    {
        public string OptionName { get; set; }
        public decimal? AdditionalPrice { get; set; }
    }
}
