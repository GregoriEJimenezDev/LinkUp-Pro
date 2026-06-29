namespace LinkUpPro.Core.Domain.Entities
{
    public class Reaction
    {
        public int Id { get; set; }
        public bool IsLike { get; set; }

        public int PostId { get; set; }
        public Post? Posts { get; set; }

        public string? UserId { get; set; }
    }
}
