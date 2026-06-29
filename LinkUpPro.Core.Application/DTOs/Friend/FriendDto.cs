namespace LinkUpPro.Core.Application.DTOs.Friend
{
    public class FriendDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public int FriendshipId { get; set; }
    }
}
