namespace LinkUpPro.Core.Domain.Entities
{
    public class Friendship
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? FirstUserId { get; set; }
        public string? SecondUserId { get; set; }
    }
}
