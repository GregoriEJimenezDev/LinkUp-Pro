namespace LinkUpPro.Core.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
                
        public int PostId { get; set; }
        public Post? Post { get; set; }

        public string? UserId { get; set; }

        public int? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<Comment> Replies { get; set; } = [];

    }
}
