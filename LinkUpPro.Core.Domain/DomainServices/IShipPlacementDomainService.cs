using LinkUpPro.Core.Domain.Enum;
using LinkUpPro.Core.Domain.ValueObjects;

namespace LinkUpPro.Core.Domain.DomainServices
{
    public interface IShipPlacementDomainService
    {
        List<BoardPosition>? CalculateCells(ShipType shipType, int row, int col, ShipDirection direction);
        bool Overlaps(List<BoardPosition> newCells, HashSet<(int Row, int Col)> occupiedCells);
    }
}
