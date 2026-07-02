using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Application.DTOs.Battleship
{
    public class ShipDto
    {
        public ShipType ShipType { get; set; }
        public int Size { get; set; }
        public bool IsSunk { get; set; }
        public List<CellDto> Cells { get; set; } = [];
    }
    public class CellDto
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }
}
