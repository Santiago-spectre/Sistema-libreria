namespace SistemaWebPapeleria.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public string CreatedAt { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
