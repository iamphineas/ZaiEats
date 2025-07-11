namespace ZaiEats.ViewModels
{
    public class AdminCommentViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public string? AdminReply { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EventTitle { get; set; }
        public int NewsEventId { get; set; }
    }

}