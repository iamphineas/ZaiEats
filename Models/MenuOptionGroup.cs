namespace ZaiEats.Models
{
    public class MenuOptionGroup
    {
        public int MenuOptionGroupId { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }

        public string GroupName { get; set; }
        public bool IsRequired { get; set; }
        public int? DisplayOrder { get; set; }

        public ICollection<MenuOptionItem> OptionItems { get; set; }
    }
}
