using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Domain.Entities
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }

        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
    }
}
