using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

public class CreateManagerViewModel
{
    [Required(ErrorMessage = "The Full Name field is required.")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "The Email field is required.")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "The Password field is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "The Restaurants field is required.")]
    public List<int> SelectedRestaurantIds { get; set; } = new(); // 🔴 Ensure it's initialized

    public List<SelectListItem> Restaurants { get; set; } = new();
}


