using LinkUpPro.Core.Application.DTOs.Battleship;

namespace LinkUpPro.Core.Application.ViewModel.Game
{
    public class BattleshipIndexViewModel
    {
        public List<GameDto> ActiveGames { get; set; } = [];
        public List<GameDto> FinishedGames { get; set; } = [];
    }
}
