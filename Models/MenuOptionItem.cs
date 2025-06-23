namespace ZaiEats.Models
{
    public class MenuOptionItem
    {
        public int MenuOptionItemId { get; set; }

        public int MenuOptionGroupId { get; set; }
        public MenuOptionGroup MenuOptionGroup { get; set; }

        public string OptionName { get; set; }
        public decimal? AdditionalPrice { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
