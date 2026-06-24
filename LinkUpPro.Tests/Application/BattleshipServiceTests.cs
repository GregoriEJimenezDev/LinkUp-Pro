using AutoMapper;
using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Domain.DomainServices;
using LinkUpPro.Core.Domain.Exceptions;
using LinkUpPro.Infrastructure.Persistence.UnitOfWork;

namespace LinkUpPro.Tests.Application;

public class BattleshipServiceTests
{
    private readonly Mock<IBattleshipGameRepository> _gameRepoMock;
    private readonly Mock<IShipRepository> _shipRepoMock;
    private readonly Mock<IAttackRepository> _attackRepoMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IShipPlacementDomainService _placementService;
    private readonly IAttackDomainService _attackDomainService;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IBattleshipService _service;

    public BattleshipServiceTests()
    {
        _gameRepoMock = new Mock<IBattleshipGameRepository>();
        _shipRepoMock = new Mock<IShipRepository>();
        _attackRepoMock = new Mock<IAttackRepository>();
        _userServiceMock = new Mock<IUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _placementService = new ShipPlacementDomainService();
        _attackDomainService = new AttackDomainService();
        _mapperMock = new Mock<IMapper>();

        _mapperMock.Setup(m => m.Map<GameDto>(It.IsAny<BattleshipGame>()))
            .Returns((BattleshipGame g) => new GameDto
            {
                Id = g.Id,
                Status = g.Status,
                Player1Id = g.FirstPlayerId,
                Player2Id = g.SecondPlayerId,
                CurrentTurnPlayerId = g.CurrentTurnPlayerId,
                StartedAt = g.StartedAt,
                FinishedAt = g.FinishedAt,
                WinnerId = g.WinnerId
            });
        _mapperMock.Setup(m => m.Map<ShipDto>(It.IsAny<Ship>()))
            .Returns((Ship s) => new ShipDto
            {
                Id = s.Id,
                ShipType = s.ShipType,
                Size = s.Size,
                IsSunk = s.IsSunk,
                PlayerId = s.PlayerId ?? "",
                Cells = s.Cells.Select(c => new CellDto { Row = c.Row, Column = c.Column, WasAttacked = c.WasAttacked }).ToList()
            });
        _mapperMock.Setup(m => m.Map<AttackDto>(It.IsAny<Attack>()))
            .Returns((Attack a) => new AttackDto
            {
                Row = a.Row,
                Column = a.Column,
                IsHit = a.IsHit,
                AttackerId = a.AttackerId ?? "",
                AttackedAt = a.AttackedAt
            });

        _service = new BattleshipService(
            _gameRepoMock.Object,
            _shipRepoMock.Object,
            _attackRepoMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _unitOfWorkMock.Object,
            _placementService,
            _attackDomainService);
    }

    [Fact]
    public async Task CreateGameAsync_ValidPlayers_ReturnsSuccess()
    {
        BattleshipGame? createdGame = null;
        _gameRepoMock.Setup(r => r.HasActiveGameWithFriendAsync("p1", "p2")).ReturnsAsync(false);
        _gameRepoMock.Setup(r => r.AddAsync(It.IsAny<BattleshipGame>()))
            .Callback<BattleshipGame>(g => { g.Id = 1; createdGame = g; })
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.CreateGameAsync("p1", "p2");

        Assert.True(result.Succeeded);
        Assert.True(result.Data > 0);
    }

    [Fact]
    public async Task CreateGameAsync_SamePlayer_ReturnsFailure()
    {
        var result = await _service.CreateGameAsync("p1", "p1");
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateGameAsync_ActiveGameExists_ReturnsFailure()
    {
        _gameRepoMock.Setup(r => r.HasActiveGameWithFriendAsync("p1", "p2")).ReturnsAsync(true);

        var result = await _service.CreateGameAsync("p1", "p2");
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task PlaceShipAsync_ValidPlacement_ReturnsSuccess()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.PlacingShips,
            FirstPlayerId = "p1",
            SecondPlayerId = "p2"
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _shipRepoMock.Setup(r => r.GetByGameAndPlayerAsync(1, "p1")).ReturnsAsync(new List<Ship>());
        _shipRepoMock.Setup(r => r.CountByGameAndPlayerAsync(1, "p2")).ReturnsAsync(5);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.PlaceShipAsync(1, "p1", "size2", 1, 1, ShipDirection.Right);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task PlaceShipAsync_DuplicateType_ReturnsFailure()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.PlacingShips,
            FirstPlayerId = "p1",
            SecondPlayerId = "p2"
        };

        var existingShips = new List<Ship>
        {
            new() { ShipType = ShipType.size2, PlayerId = "p1", Cells = new List<ShipCell> { new() { Row = 5, Column = 5 } } }
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _shipRepoMock.Setup(r => r.GetByGameAndPlayerAsync(1, "p1")).ReturnsAsync(existingShips);

        var result = await _service.PlaceShipAsync(1, "p1", "size2", 1, 1, ShipDirection.Right);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task PlaceShipAsync_OutOfBounds_ReturnsFailure()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.PlacingShips,
            FirstPlayerId = "p1",
            SecondPlayerId = "p2"
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _shipRepoMock.Setup(r => r.GetByGameAndPlayerAsync(1, "p1")).ReturnsAsync(new List<Ship>());

        var result = await _service.PlaceShipAsync(1, "p1", "size5", 10, 10, ShipDirection.Down);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task AttackAsync_ValidAttack_ReturnsSuccess()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.InProgress,
            CurrentTurnPlayerId = "p1",
            FirstPlayerId = "p1",
            SecondPlayerId = "p2"
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _attackRepoMock.Setup(r => r.GetByGameAndAttackerAsync(1, "p1")).ReturnsAsync(new List<Attack>());
        _shipRepoMock.Setup(r => r.GetWithCellsByGameAndPlayerAsync(1, "p2")).ReturnsAsync(new List<Ship>());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.AttackAsync(1, "p1", 3, 3);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task AttackAsync_WrongTurn_ReturnsFailure()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.InProgress,
            CurrentTurnPlayerId = "p1",
            FirstPlayerId = "p1",
            SecondPlayerId = "p2"
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);

        var result = await _service.AttackAsync(1, "p2", 3, 3);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task AttackAsync_DuplicateCell_ReturnsFailure()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.InProgress,
            CurrentTurnPlayerId = "p1",
            FirstPlayerId = "p1",
            SecondPlayerId = "p2"
        };

        var existingAttacks = new List<Attack>
        {
            new() { Row = 3, Column = 3, AttackerId = "p1", GameId = 1 }
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _attackRepoMock.Setup(r => r.GetByGameAndAttackerAsync(1, "p1")).ReturnsAsync(existingAttacks);

        var result = await _service.AttackAsync(1, "p1", 3, 3);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SurrenderAsync_ActiveGame_ReturnsSuccess()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.InProgress,
            FirstPlayerId = "p1",
            SecondPlayerId = "p2",
            CurrentTurnPlayerId = "p1"
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.SurrenderAsync(1, "p1");

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task SurrenderAsync_FinishedGame_ReturnsFailure()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.Finished,
            FirstPlayerId = "p1",
            SecondPlayerId = "p2"
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);

        var result = await _service.SurrenderAsync(1, "p1");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task AttackAsync_ConcurrencyException_ReturnsFailure()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.InProgress,
            CurrentTurnPlayerId = "p1",
            FirstPlayerId = "p1",
            SecondPlayerId = "p2"
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _attackRepoMock.Setup(r => r.GetByGameAndAttackerAsync(1, "p1")).ReturnsAsync(new List<Attack>());
        _shipRepoMock.Setup(r => r.GetWithCellsByGameAndPlayerAsync(1, "p2")).ReturnsAsync(new List<Ship>());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ThrowsAsync(new ConcurrencyException());

        var result = await _service.AttackAsync(1, "p1", 3, 3);

        Assert.False(result.Succeeded);
        Assert.Contains("modified", result.ErrorMessage);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsCorrectStats()
    {
        var userId = "p1";
        var allGames = new List<BattleshipGame>
        {
            new() { Id = 1, Status = GameStatus.Finished, WinnerId = "p1", FirstPlayerId = "p1", SecondPlayerId = "p2" },
            new() { Id = 2, Status = GameStatus.Finished, WinnerId = "p2", FirstPlayerId = "p1", SecondPlayerId = "p2" },
            new() { Id = 3, Status = GameStatus.InProgress, FirstPlayerId = "p1", SecondPlayerId = "p2" }
        };

        var allAttacks = new List<Attack>
        {
            new() { AttackerId = "p1", IsHit = true },
            new() { AttackerId = "p1", IsHit = true },
            new() { AttackerId = "p1", IsHit = false },
            new() { AttackerId = "p1", IsHit = true }
        };

        _gameRepoMock.Setup(r => r.GetByPlayerAsync(userId)).ReturnsAsync(allGames);
        _attackRepoMock.Setup(r => r.GetByAttackerAsync(userId)).ReturnsAsync(allAttacks);

        var stats = await _service.GetStatsAsync(userId);

        Assert.Equal(3, stats.TotalGames);
        Assert.Equal(1, stats.WonGames);
        Assert.Equal(1, stats.LostGames);
        Assert.Equal(50, stats.WinRatio);
        Assert.Equal(4, stats.TotalAttacks);
        Assert.Equal(3, stats.TotalHits);
        Assert.Equal(75, stats.Accuracy);
    }

    [Fact]
    public async Task AttackAsync_HitSinksShipAndWins_ReturnsSuccess()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.InProgress,
            CurrentTurnPlayerId = "p1",
            FirstPlayerId = "p1",
            SecondPlayerId = "p2"
        };

        var opponentShips = new List<Ship>
        {
            new()
            {
                Id = 1,
                IsSunk = false,
                Cells = new List<ShipCell>
                {
                    new() { Id = 1, Row = 3, Column = 3, WasAttacked = false, ShipId = 1 },
                    new() { Id = 2, Row = 3, Column = 4, WasAttacked = true, ShipId = 1 }
                }
            }
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _attackRepoMock.Setup(r => r.GetByGameAndAttackerAsync(1, "p1")).ReturnsAsync(new List<Attack>());
        _shipRepoMock.Setup(r => r.GetWithCellsByGameAndPlayerAsync(1, "p2")).ReturnsAsync(opponentShips);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.AttackAsync(1, "p1", 3, 3);

        Assert.True(result.Succeeded);
        Assert.Equal(GameStatus.Finished, game.Status);
        Assert.Equal("p1", game.WinnerId);
    }

    [Fact]
    public async Task GetIndexAsync_ReturnsCorrectViewModels()
    {
        var userId = "p1";
        var activeGames = new List<BattleshipGame>
        {
            new() { Id = 1, Status = GameStatus.InProgress, FirstPlayerId = "p1", SecondPlayerId = "p2", StartedAt = DateTime.UtcNow },
            new() { Id = 2, Status = GameStatus.PlacingShips, FirstPlayerId = "p1", SecondPlayerId = "p3", StartedAt = DateTime.UtcNow }
        };
        var finishedGames = new List<BattleshipGame>
        {
            new() { Id = 3, Status = GameStatus.Finished, FirstPlayerId = "p1", SecondPlayerId = "p4", WinnerId = "p1", StartedAt = DateTime.UtcNow, FinishedAt = DateTime.UtcNow }
        };

        _gameRepoMock.Setup(r => r.GetActiveByUserIdAsync(userId)).ReturnsAsync(activeGames);
        _gameRepoMock.Setup(r => r.GetFinishedByUserIdAsync(userId)).ReturnsAsync(finishedGames);
        _userServiceMock.Setup(u => u.GetUserBasicInfoAsync(It.IsAny<string>())).ReturnsAsync(new UserBasicDto { Username = "testuser" });

        var vm = await _service.GetIndexAsync(userId);

        Assert.Equal(2, vm.ActiveGames.Count);
        Assert.Single(vm.FinishedGames);
        Assert.Equal(1, vm.TotalGames);
        Assert.Equal(1, vm.WonGames);
        Assert.Equal(0, vm.LostGames);
    }

    [Fact]
    public async Task PlaceShipAsync_BothPlayersPlaced_StartsGame()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.PlacingShips,
            FirstPlayerId = "p1",
            SecondPlayerId = "p2",
            CurrentTurnPlayerId = "p1"
        };

        var existingShips = new List<Ship>
        {
            new() { ShipType = ShipType.size2, PlayerId = "p1", Size = 2, Cells = new List<ShipCell> { new() { Row = 1, Column = 1 } } },
            new() { ShipType = ShipType.size3A, PlayerId = "p1", Size = 3, Cells = new List<ShipCell> { new() { Row = 2, Column = 1 } } },
            new() { ShipType = ShipType.size3B, PlayerId = "p1", Size = 3, Cells = new List<ShipCell> { new() { Row = 3, Column = 1 } } },
            new() { ShipType = ShipType.size4, PlayerId = "p1", Size = 4, Cells = new List<ShipCell> { new() { Row = 4, Column = 1 } } }
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _shipRepoMock.Setup(r => r.GetByGameAndPlayerAsync(1, "p1")).ReturnsAsync(existingShips);
        _shipRepoMock.Setup(r => r.CountByGameAndPlayerAsync(1, "p2")).ReturnsAsync(5);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.PlaceShipAsync(1, "p1", "size5", 8, 8, ShipDirection.Right);

        Assert.True(result.Succeeded);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public async Task AttackAsync_SwitchesTurnAfterMiss()
    {
        var game = new BattleshipGame
        {
            Id = 1,
            Status = GameStatus.InProgress,
            CurrentTurnPlayerId = "p1",
            FirstPlayerId = "p1",
            SecondPlayerId = "p2"
        };

        var opponentShips = new List<Ship>
        {
            new()
            {
                Id = 1,
                IsSunk = false,
                Cells = new List<ShipCell>
                {
                    new() { Row = 3, Column = 3, WasAttacked = false, ShipId = 1 },
                    new() { Row = 4, Column = 3, WasAttacked = false, ShipId = 1 }
                }
            }
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _attackRepoMock.Setup(r => r.GetByGameAndAttackerAsync(1, "p1")).ReturnsAsync(new List<Attack>());
        _shipRepoMock.Setup(r => r.GetWithCellsByGameAndPlayerAsync(1, "p2")).ReturnsAsync(opponentShips);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.AttackAsync(1, "p1", 5, 5);

        Assert.True(result.Succeeded);
        Assert.Equal("p2", game.CurrentTurnPlayerId);
        Assert.NotEqual(GameStatus.Finished, game.Status);
    }

    [Fact]
    public async Task GetPendingShipsAsync_ReturnsCorrectPendingList()
    {
        var gameId = 1;
        var playerId = "p1";
        var game = new BattleshipGame
        {
            Id = gameId,
            FirstPlayerId = playerId,
            SecondPlayerId = "p2",
            Status = GameStatus.PlacingShips
        };

        var placedShips = new List<Ship>
        {
            new() { ShipType = ShipType.size2, PlayerId = playerId, Size = 2, Cells = new List<ShipCell> { new() { Row = 1, Column = 1 } } },
            new() { ShipType = ShipType.size3A, PlayerId = playerId, Size = 3, Cells = new List<ShipCell> { new() { Row = 2, Column = 1 } } }
        };

        game.Ships = placedShips;

        _gameRepoMock.Setup(r => r.GetWithDetailsAsync(gameId)).ReturnsAsync(game);

        var vm = await _service.GetPendingShipsAsync(gameId, playerId);

        Assert.Equal(2, vm.PlacedShips.Count);
        Assert.Equal(3, vm.PendingShips.Count);
        Assert.Contains(vm.PendingShips, s => s.ShipType == ShipType.size5);
    }
}
