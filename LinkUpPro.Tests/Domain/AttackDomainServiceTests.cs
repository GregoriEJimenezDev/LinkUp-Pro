namespace LinkUpPro.Tests.Domain;

public class AttackDomainServiceTests
{
    private readonly AttackDomainService _service;

    public AttackDomainServiceTests()
    {
        _service = new AttackDomainService();
    }

    private BattleshipGame CreateInProgressGame(string currentTurnId = "player1")
    {
        return new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.InProgress,
            CurrentTurnPlayerId = currentTurnId,
            FirstPlayerId = "player1",
            SecondPlayerId = "player2"
        };
    }

    [Fact]
    public void ValidateTurn_CorrectPlayer_ReturnsSuccess()
    {
        var game = CreateInProgressGame("player1");
        var result = _service.ValidateTurn(game, "player1");
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void ValidateTurn_WrongPlayer_ReturnsFailure()
    {
        var game = CreateInProgressGame("player1");
        var result = _service.ValidateTurn(game, "player2");
        Assert.False(result.Succeeded);
        Assert.Contains("not your turn", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateTurn_GameNotInProgress_ReturnsFailure()
    {
        var game = CreateInProgressGame();
        game.Status = GameStatus.PlacingShips;
        var result = _service.ValidateTurn(game, "player1");
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void ValidateTurn_FinishedGame_ReturnsFailure()
    {
        var game = CreateInProgressGame();
        game.Status = GameStatus.Finished;
        var result = _service.ValidateTurn(game, "player1");
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void IsDuplicateAttack_ExistingAttack_ReturnsTrue()
    {
        var attacks = new List<Attack>
        {
            new() { Row = 3, Column = 5 }
        };
        Assert.True(_service.IsDuplicateAttack(attacks, 3, 5));
    }

    [Fact]
    public void IsDuplicateAttack_NewCell_ReturnsFalse()
    {
        var attacks = new List<Attack>
        {
            new() { Row = 3, Column = 5 }
        };
        Assert.False(_service.IsDuplicateAttack(attacks, 4, 5));
    }

    [Fact]
    public void IsDuplicateAttack_EmptyList_ReturnsFalse()
    {
        Assert.False(_service.IsDuplicateAttack(new List<Attack>(), 1, 1));
    }

    [Fact]
    public void EvaluateImpact_Hit_ReturnsCorrectResult()
    {
        var ship = new Ship
        {
            Id = 1,
            Cells = new List<ShipCell>
            {
                new() { Row = 3, Column = 3 },
                new() { Row = 3, Column = 4 }
            }
        };
        var result = _service.EvaluateImpact(new[] { ship }, 3, 3);
        Assert.True(result.IsHit);
        Assert.Equal(ship, result.HitShip);
        Assert.NotNull(result.HitCell);
        Assert.Equal(3, result.HitCell.Row);
        Assert.Equal(3, result.HitCell.Column);
    }

    [Fact]
    public void EvaluateImpact_Miss_ReturnsNotHit()
    {
        var ship = new Ship
        {
            Id = 1,
            Cells = new List<ShipCell>
            {
                new() { Row = 3, Column = 3 }
            }
        };
        var result = _service.EvaluateImpact(new[] { ship }, 5, 5);
        Assert.False(result.IsHit);
        Assert.Null(result.HitShip);
    }

    [Fact]
    public void CheckIfSunk_AllCellsAttacked_ReturnsTrue()
    {
        var ship = new Ship
        {
            Cells = new List<ShipCell>
            {
                new() { Row = 1, Column = 1, WasAttacked = true },
                new() { Row = 1, Column = 2, WasAttacked = true }
            }
        };
        Assert.True(_service.CheckIfSunk(ship));
    }

    [Fact]
    public void CheckIfSunk_NotAllAttacked_ReturnsFalse()
    {
        var ship = new Ship
        {
            Cells = new List<ShipCell>
            {
                new() { Row = 1, Column = 1, WasAttacked = true },
                new() { Row = 1, Column = 2, WasAttacked = false }
            }
        };
        Assert.False(_service.CheckIfSunk(ship));
    }

    [Fact]
    public void CheckVictory_AllShipsSunk_ReturnsTrue()
    {
        var ships = new List<Ship>
        {
            new() { IsSunk = true },
            new() { IsSunk = true }
        };
        Assert.True(_service.CheckVictory(ships));
    }

    [Fact]
    public void CheckVictory_SomeShipsAfloat_ReturnsFalse()
    {
        var ships = new List<Ship>
        {
            new() { IsSunk = true },
            new() { IsSunk = false }
        };
        Assert.False(_service.CheckVictory(ships));
    }
}
