using LinkUpPro.Core.Application.DTOs.Comment;
using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Application.ViewModel.Post
{
    public class PostViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public MediaType MediaType { get; set; }
        public string? MediaUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? UserProfilePicture { get; set; }
        public PostPrivacy Privacy { get; set; }
        public bool AllowComments { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public bool? CurrentUserReaction { get; set; }
        public List<CommentDto> Comments { get; set; } = [];
    }
}
