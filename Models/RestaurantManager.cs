namespace ZaiEats.Models
{
    public class RestaurantManager
    {
        public int RestaurantManagerId { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }

        public string ManagerId { get; set; }
        public ApplicationUser Manager { get; set; }
    }

}
