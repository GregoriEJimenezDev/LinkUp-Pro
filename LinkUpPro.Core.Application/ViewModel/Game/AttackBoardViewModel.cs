using LinkUpPro.Core.Application.DTOs.Battleship;

namespace LinkUpPro.Core.Application.ViewModel.Game
{
    public class AttackBoardViewModel
    {
        public int GameId { get; set; }
        public bool IsMyTurn { get; set; }
        public string OpponentUsername { get; set; } = string.Empty;
        public string CurrentTurnUsername { get; set; } = string.Empty;

        public List<AttackDto> MyAttacks { get; set; } = [];
    }
}
