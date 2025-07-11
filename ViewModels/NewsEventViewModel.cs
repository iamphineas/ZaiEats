using System.ComponentModel.DataAnnotations;

namespace ZaiEats.ViewModels
{
    public class NewsEventViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public DateTime EventDateTime { get; set; }

        public string Description { get; set; }
        public string Location { get; set; }
        public string DressCode { get; set; }

        [Required]
        public string Category { get; set; }

        public IFormFile ImageFile { get; set; } // For image upload
    }

}
