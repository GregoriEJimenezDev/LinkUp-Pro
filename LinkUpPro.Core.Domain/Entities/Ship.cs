using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Domain.Entities
{
    public class Ship
    {
        public static readonly Dictionary<ShipType, int> ShipSizes = new()
        {
            { ShipType.size2, 2 },
            { ShipType.size3A, 3 },
            { ShipType.size1, 1 },
            { ShipType.size4, 4 },
            { ShipType.size5, 5 }
        };

        public int Id { get; set; }
        public ShipType ShipType { get; set; }
        public int Size { get; set; }
        public bool IsSunk { get; set; } = false;

        public int GameId { get; set; }
        public BattleshipGame? Game { get; set; }
        public string? PlayerId { get; set; }
        public ICollection<ShipCell> Cells { get; set; } = [];

        public bool IsCellPartOfShip(int row, int col) =>
            Cells.Any(c => c.Row == row && c.Column == col);

        public bool CheckIfSunk() =>
            Cells.Count > 0 && Cells.All(c => c.WasAttacked);
    }
}
