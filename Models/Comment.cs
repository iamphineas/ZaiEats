namespace ZaiEats.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string UserName { get; set; } // or pull from Identity if logged in
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int NewsEventId { get; set; }
        public NewsEvent NewsEvent { get; set; }

        // Admin reply
        public string? AdminReply { get; set; }
    }
}
