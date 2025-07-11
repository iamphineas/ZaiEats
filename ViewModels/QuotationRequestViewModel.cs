using System.ComponentModel.DataAnnotations;

namespace ZaiEats.ViewModels
{
    public class QuotationRequestViewModel
    {
        [Required]
        public string EventType { get; set; }

        [Required]
        public int NumberOfGuests { get; set; }

        [Required]
        public DateTime EventDateTime { get; set; }

        public string EventDescription { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string MobileNumber { get; set; }

        [Required]
        public string EventLocation { get; set; }
    }
}

