using LinkUpPro.Core.Application.DTOs.Battleship;

namespace LinkUpPro.Core.Application.ViewModel.Game
{
    public class GameResultViewModel
    {
        public int GameId { get; set; }
        public string OpponentUsername { get; set; } = string.Empty;
        public bool IWon { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }


        public List<AttackDto> MyAttacks { get; set; } = [];
        public List<AttackDto> OpponentAttacks { get; set; } = [];
        public List<ShipDto> MyShips { get; set; } = new();
        public int EnemyShipsSunk { get; set; }
        public bool WonBySurrender { get; set; }
    }
}
