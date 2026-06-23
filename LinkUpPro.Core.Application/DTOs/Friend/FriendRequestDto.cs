using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Application.DTOs.Friend
{
    public class FriendRequestDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderUsername { get; set; } = string.Empty;
        public string? SenderProfilePicture { get; set; }
        public string ReceiverId { get; set; } = string.Empty;
        public string ReceiverUsername { get; set; } = string.Empty;
        public FriendRequestStatus Status { get; set; }
        public DateTime SentAt { get; set; }
        public int MutualFriendsCount { get; set; }
    }
}
