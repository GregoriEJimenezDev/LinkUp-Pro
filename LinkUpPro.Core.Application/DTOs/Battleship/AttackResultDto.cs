namespace LinkUpPro.Core.Application.DTOs.Battleship
{
    public class AttackResultDto
    {
        public bool IsHit { get; set; }
        public bool IsSunk { get; set; }
        public bool IsVictory { get; set; }
        public string? SunkShipType { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
