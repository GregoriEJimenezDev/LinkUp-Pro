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

        public byte[] ConcurrencyStamp { get; set; } = [];

        // Navigation properties
        public ICollection<Ship> Ships { get; set; } = [];
        public ICollection<Attack> Attacks { get; set; } = [];

        public bool CanAttack(string playerId) =>
            Status == GameStatus.InProgress && CurrentTurnPlayerId == playerId;

        public void SwitchTurn() =>
            CurrentTurnPlayerId = CurrentTurnPlayerId == FirstPlayerId ? SecondPlayerId : FirstPlayerId;

        public void SetWinner(string playerId)
        {
            Status = GameStatus.Finished;
            WinnerId = playerId;
            FinishedAt = DateTime.UtcNow;
        }

        public void SetSurrender(string playerId)
        {
            Status = GameStatus.Finished;
            WinnerId = FirstPlayerId == playerId ? SecondPlayerId : FirstPlayerId;
            FinishedAt = DateTime.UtcNow;
        }

        public bool CheckTimeout(int hours = 48)
        {
            if (!LastAttackedAt.HasValue)
                return false;

            return (DateTime.UtcNow - LastAttackedAt.Value).TotalHours >= hours;
        }

        public bool BothPlayersPlacedShips(int playerShipCount, int opponentShipCount) =>
            playerShipCount >= 5 && opponentShipCount >= 5;

        public void StartGame()
        {
            Status = GameStatus.InProgress;
            CurrentTurnPlayerId = FirstPlayerId;
            LastAttackedAt = DateTime.UtcNow;
        }
    }
}
