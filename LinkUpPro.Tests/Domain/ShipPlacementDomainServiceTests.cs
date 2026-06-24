namespace LinkUpPro.Tests.Domain;

public class ShipPlacementDomainServiceTests
{
    private readonly ShipPlacementDomainService _service;

    public ShipPlacementDomainServiceTests()
    {
        _service = new ShipPlacementDomainService();
    }

    [Fact]
    public void CalculateCells_Size2_Right_ReturnsCorrectPositions()
    {
        var cells = _service.CalculateCells(ShipType.size2, 5, 5, ShipDirection.Right);
        Assert.NotNull(cells);
        Assert.Equal(2, cells.Count);
        Assert.Equal(5, cells[0].Row);
        Assert.Equal(5, cells[0].Column);
        Assert.Equal(5, cells[1].Row);
        Assert.Equal(6, cells[1].Column);
    }

    [Fact]
    public void CalculateCells_Size5_Down_ReturnsCorrectPositions()
    {
        var cells = _service.CalculateCells(ShipType.size5, 1, 1, ShipDirection.Down);
        Assert.NotNull(cells);
        Assert.Equal(5, cells.Count);
        Assert.Equal(5, cells[4].Row);
        Assert.Equal(1, cells[4].Column);
    }

    [Fact]
    public void CalculateCells_OutOfBounds_ReturnsNull()
    {
        var cells = _service.CalculateCells(ShipType.size5, 10, 1, ShipDirection.Down);
        Assert.Null(cells);
    }

    [Fact]
    public void CalculateCells_UpFromEdge_ReturnsNull()
    {
        var cells = _service.CalculateCells(ShipType.size3A, 2, 5, ShipDirection.Up);
        Assert.Null(cells);
    }

    [Fact]
    public void CalculateCells_LeftFromEdge_ReturnsNull()
    {
        var cells = _service.CalculateCells(ShipType.size4, 5, 2, ShipDirection.Left);
        Assert.Null(cells);
    }

    [Fact]
    public void CalculateCells_BoundaryRight_Valid()
    {
        var cells = _service.CalculateCells(ShipType.size3A, 5, 10, ShipDirection.Right);
        Assert.NotNull(cells);
        Assert.Equal(3, cells.Count);
    }

    [Fact]
    public void CalculateCells_BoundaryDown_Valid()
    {
        var cells = _service.CalculateCells(ShipType.size4, 9, 5, ShipDirection.Down);
        Assert.NotNull(cells);
        Assert.Equal(4, cells.Count);
    }

    [Fact]
    public void CalculateCells_DefaultShipSizes_AllCorrect()
    {
        Assert.Equal(2, Ship.ShipSizes[ShipType.size2]);
        Assert.Equal(3, Ship.ShipSizes[ShipType.size3A]);
        Assert.Equal(3, Ship.ShipSizes[ShipType.size3B]);
        Assert.Equal(4, Ship.ShipSizes[ShipType.size4]);
        Assert.Equal(5, Ship.ShipSizes[ShipType.size5]);
    }

    [Fact]
    public void Overlaps_OverlappingCells_ReturnsTrue()
    {
        var newCells = new List<BoardPosition>
        {
            new(3, 3),
            new(3, 4)
        };
        var occupied = new HashSet<(int, int)> { (3, 4), (5, 5) };
        Assert.True(_service.Overlaps(newCells, occupied));
    }

    [Fact]
    public void Overlaps_NoOverlap_ReturnsFalse()
    {
        var newCells = new List<BoardPosition>
        {
            new(1, 1),
            new(1, 2)
        };
        var occupied = new HashSet<(int, int)> { (3, 3), (4, 4) };
        Assert.False(_service.Overlaps(newCells, occupied));
    }

    [Fact]
    public void Overlaps_EmptyOccupied_ReturnsFalse()
    {
        var newCells = new List<BoardPosition> { new(1, 1) };
        var occupied = new HashSet<(int, int)>();
        Assert.False(_service.Overlaps(newCells, occupied));
    }

    [Fact]
    public void CalculateCells_UpDirection_CalculatesCorrectly()
    {
        var cells = _service.CalculateCells(ShipType.size3A, 5, 5, ShipDirection.Up);
        Assert.NotNull(cells);
        Assert.Equal(5, cells[0].Row);
        Assert.Equal(4, cells[1].Row);
        Assert.Equal(3, cells[2].Row);
    }

    [Fact]
    public void CalculateCells_LeftDirection_CalculatesCorrectly()
    {
        var cells = _service.CalculateCells(ShipType.size3A, 5, 5, ShipDirection.Left);
        Assert.NotNull(cells);
        Assert.Equal(5, cells[0].Row);
        Assert.Equal(5, cells[0].Column);
        Assert.Equal(4, cells[1].Column);
        Assert.Equal(3, cells[2].Column);
    }

    [Fact]
    public void CalculateCells_AllDirections_Size3_AllValid()
    {
        var right = _service.CalculateCells(ShipType.size3A, 5, 5, ShipDirection.Right);
        var down = _service.CalculateCells(ShipType.size3A, 5, 5, ShipDirection.Down);
        var left = _service.CalculateCells(ShipType.size3A, 5, 5, ShipDirection.Left);
        var up = _service.CalculateCells(ShipType.size3A, 5, 5, ShipDirection.Up);

        Assert.NotNull(right);
        Assert.NotNull(down);
        Assert.NotNull(left);
        Assert.NotNull(up);
        Assert.Equal(3, right.Count);
        Assert.Equal(3, down.Count);
        Assert.Equal(3, left.Count);
        Assert.Equal(3, up.Count);
    }
}
