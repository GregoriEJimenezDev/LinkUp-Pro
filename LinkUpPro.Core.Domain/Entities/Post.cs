using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public MediaType MediaType { get; set; }
        public string? MediaUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string? UserId { get; set; }

        // Navigation properties
        public ICollection<Comment> Comments { get; set; } = [];
        public ICollection<Reaction> Reactions { get; set; } = [];
    }
}
