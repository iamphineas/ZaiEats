namespace ZaiEats.ViewModels
{
    public class AssignManagerViewModel
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public List<ManagerCheckbox> AllManagers { get; set; }
    }
}
