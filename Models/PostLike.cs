namespace ZaiEats.Models
{
    public class PostLike
    {
        public int Id { get; set; }

        public int NewsEventId { get; set; }

        public string UserName { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}
