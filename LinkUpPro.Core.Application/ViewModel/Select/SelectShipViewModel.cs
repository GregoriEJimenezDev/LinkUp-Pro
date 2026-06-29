using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Application.ViewModel.Select
{
    public class SelectShipViewModel
    {
        public int GameId { get; set; }
        public List<ShipDto> PendingShips { get; set; } = [];
        public List<ShipDto> PlacedShips { get; set; } = [];
    }
}
