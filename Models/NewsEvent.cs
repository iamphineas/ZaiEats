namespace ZaiEats.Models
{
    public class NewsEvent
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime EventDateTime { get; set; }

        public string Location { get; set; }

        public string DressCode { get; set; }

        // 🚫 DO NOT mark this as [Required]
        public string? ImageUrl { get; set; }

        public string Category { get; set; }

        public int Likes { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}

