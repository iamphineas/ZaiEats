using System.ComponentModel.DataAnnotations;

namespace ZaiEats.Models
{
    public class QuotationRequest
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Event Type")]
        public string EventType { get; set; }

        [Required]
        [Display(Name = "Number Of Guests")]
        public int NumberOfGuests { get; set; }

        [Required]
        [Display(Name = "Event Date & Time")]
        public DateTime EventDateTime { get; set; }

        [Display(Name = "Event Description")]
        public string EventDescription { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required, Phone]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [Required]
        [Display(Name = "Event Location")]
        public string EventLocation { get; set; }

        public string Status { get; set; } = "Pending"; // or "Quotation Sent"
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
