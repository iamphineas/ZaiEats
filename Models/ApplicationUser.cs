using Microsoft.AspNetCore.Identity;

namespace ZaiEats.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public ICollection<RestaurantManager> RestaurantManagers { get; set; }

    }
}
