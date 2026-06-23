namespace LinkUpPro.Core.Application.DTOs.Battleship
{
    public class AttackDto
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsHit { get; set; }
        public string AttackerId { get; set; } = string.Empty;
        public DateTime AttackedAt { get; set; }
    }
}
