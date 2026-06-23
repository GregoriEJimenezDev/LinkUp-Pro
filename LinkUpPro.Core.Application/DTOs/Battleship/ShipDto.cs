using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Application.DTOs.Battleship
{
    public class ShipDto
    {
        public int Id { get; set; }
        public ShipType ShipType { get; set; }
        public int Size { get; set; }
        public bool IsSunk { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public List<CellDto> Cells { get; set; } = [];
    }
    public class CellDto
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public bool WasAttacked { get; set; }
    }
}
