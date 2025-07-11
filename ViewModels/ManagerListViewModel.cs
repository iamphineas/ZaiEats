namespace ZaiEats.ViewModels
{
    public class ManagerListViewModel
    {
        public string ManagerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<string> AssignedRestaurants { get; set; }
    }

}
