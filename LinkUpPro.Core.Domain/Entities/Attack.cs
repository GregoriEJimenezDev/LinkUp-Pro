namespace LinkUpPro.Core.Domain.Entities
{
    public class Attack
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsHit { get; set; }
        public DateTime AttackedAt { get; set; } = DateTime.UtcNow;

        public int GameId { get; set; }
        public BattleshipGame? Game { get; set; }

        public string? AttackerId { get; set; }

        public bool IsSamePosition(int row, int col) =>
            Row == row && Column == col;
    }
}
