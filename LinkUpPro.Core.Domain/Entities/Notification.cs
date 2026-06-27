using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Url { get; set; }
        public NotificationType Type { get; set; } = NotificationType.FriendRequestReceived;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
