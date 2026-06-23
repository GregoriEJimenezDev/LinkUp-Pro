using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Domain.Entities
{
    public class BattleshipGame
    {
        public int Id { get; set; }
        public GameStatus Status { get; set; } = GameStatus.PlacingShips;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; }
        public DateTime? LastAttackedAt { get; set; }

        public string CurrentTurnPlayerId { get; set; } = string.Empty;

        public string FirstPlayerId { get; set; } = string.Empty;
        public string SecondPlayerId { get; set; } = string.Empty;
        public string WinnerId { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Ship> Ships { get; set; } =[];
        public ICollection<Attack> Attacks { get; set; } = [];
    }
}
