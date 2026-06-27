using LinkUpPro.Core.Application.DTOs.Battleship;

namespace LinkUpPro.Core.Application.ViewModel.Game
{
    public class AttackBoardViewModel
    {
        public int GameId { get; set; }
        public bool IsMyTurn { get; set; }
        public bool IsFinished { get; set; }
        public string OpponentUsername { get; set; } = string.Empty;
        public string CurrentTurnUsername { get; set; } = string.Empty;

        public List<AttackDto> MyAttacks { get; set; } = [];
        public List<ShipDto> MyShips { get; set; } = [];
        public List<AttackDto> OpponentAttacks { get; set; } = [];
        public List<ShipDto> OpponentSunkenShips { get; set; } = [];
    }
}
