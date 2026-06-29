using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Application.DTOs.Battleship
{
    public class GameDto
    {
        public int Id { get; set; }
        public string Player1Id { get; set; } = string.Empty;
        public string Player1Username { get; set; } = string.Empty;
        public string Player2Id { get; set; } = string.Empty;
        public string Player2Username { get; set; } = string.Empty;
        public string CurrentTurnPlayerId { get; set; } = string.Empty;
        public GameStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string? WinnerId { get; set; }
        public string? WinnerUsername { get; set; }
        public double HoursElapsed { get; set; }
    }
}
