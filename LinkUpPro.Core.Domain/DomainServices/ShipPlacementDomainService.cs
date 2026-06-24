using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enum;
using LinkUpPro.Core.Domain.ValueObjects;

namespace LinkUpPro.Core.Domain.DomainServices
{
    public class ShipPlacementDomainService : IShipPlacementDomainService
    {
        public List<BoardPosition>? CalculateCells(ShipType shipType, int row, int col, ShipDirection direction)
        {
            var size = Ship.ShipSizes[shipType];
            var cells = new List<BoardPosition>();

            for (int i = 0; i < size; i++)
            {
                int r = row, c = col;

                switch (direction)
                {
                    case ShipDirection.Up:
                        r = row - i;
                        break;
                    case ShipDirection.Down:
                        r = row + i;
                        break;
                    case ShipDirection.Left:
                        c = col - i;
                        break;
                    case ShipDirection.Right:
                        c = col + i;
                        break;
                }

                var pos = new BoardPosition(r, c);
                if (!pos.IsWithinBoard)
                    return null;

                cells.Add(pos);
            }

            return cells;
        }

        public bool Overlaps(List<BoardPosition> newCells, HashSet<(int Row, int Col)> occupiedCells)
        {
            return newCells.Any(c => occupiedCells.Contains((c.Row, c.Column)));
        }
    }
}
