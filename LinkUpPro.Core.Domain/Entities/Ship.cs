using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Domain.Entities
{
    public class Ship
    {
        public int Id { get; set; }
        public ShipType ShipType { get; set; }
        public int Size { get; set; }
        public bool IsSunk { get; set; } = false;

        public int GameId { get; set; }
        public BattleshipGame? Game { get; set; }
        public string? PlayerId { get; set; }

        //navigation property 
        public ICollection<ShipCell> Cells { get; set; } = [];
    }
}
