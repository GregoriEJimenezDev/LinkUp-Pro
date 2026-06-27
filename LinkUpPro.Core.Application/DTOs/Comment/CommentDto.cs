namespace LinkUpPro.Core.Application.DTOs.Comment
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? UserProfilePicture { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        public List<CommentDto> Replies { get; set; } = [];
    }
}
