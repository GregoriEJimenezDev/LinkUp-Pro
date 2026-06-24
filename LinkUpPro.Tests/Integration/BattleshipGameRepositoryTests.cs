using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Integration;

public class BattleshipGameRepositoryTests
{
    private LinkUpProDbContext CreateDbContext(string name)
    {
        var options = new DbContextOptionsBuilder<LinkUpProDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new LinkUpProDbContext(options);
    }

    private BattleshipGame CreateGame(int id, string p1, string p2, GameStatus status)
    {
        return new BattleshipGame
        {
            Id = id,
            FirstPlayerId = p1,
            SecondPlayerId = p2,
            Status = status,
            CurrentTurnPlayerId = p1,
            StartedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task AddAsync_GamePersisted()
    {
        using var context = CreateDbContext("AddAsync_GamePersisted");
        var repo = new BattleshipGameRepository(context);

        var game = CreateGame(1, "p1", "p2", GameStatus.PlacingShips);
        await repo.AddAsync(game);
        await context.SaveChangesAsync();

        var retrieved = await repo.GetByIdAsync(1);
        Assert.NotNull(retrieved);
        Assert.Equal(GameStatus.PlacingShips, retrieved.Status);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ReturnsActiveGames()
    {
        using var context = CreateDbContext("GetActiveByUserIdAsync");
        var repo = new BattleshipGameRepository(context);

        context.BattleshipGames.Add(CreateGame(1, "p1", "p2", GameStatus.InProgress));
        context.BattleshipGames.Add(CreateGame(2, "p3", "p1", GameStatus.Finished));
        context.BattleshipGames.Add(CreateGame(3, "p1", "p4", GameStatus.PlacingShips));
        await context.SaveChangesAsync();

        var active = await repo.GetActiveByUserIdAsync("p1");
        Assert.Equal(2, active.Count());
    }

    [Fact]
    public async Task GetFinishedByUserIdAsync_ReturnsFinishedGames()
    {
        using var context = CreateDbContext("GetFinishedByUserIdAsync");
        var repo = new BattleshipGameRepository(context);

        var game = CreateGame(1, "p1", "p2", GameStatus.Finished);
        game.FinishedAt = DateTime.UtcNow;
        game.WinnerId = "p1";
        context.BattleshipGames.Add(game);
        context.BattleshipGames.Add(CreateGame(2, "p1", "p3", GameStatus.InProgress));
        await context.SaveChangesAsync();

        var finished = await repo.GetFinishedByUserIdAsync("p1");
        Assert.Single(finished);
    }

    [Fact]
    public async Task HasActiveGameWithFriendAsync_ReturnsTrue()
    {
        using var context = CreateDbContext("HasActiveGameWithFriend");
        var repo = new BattleshipGameRepository(context);

        context.BattleshipGames.Add(CreateGame(1, "p1", "p2", GameStatus.InProgress));
        await context.SaveChangesAsync();

        var result = await repo.HasActiveGameWithFriendAsync("p1", "p2");
        Assert.True(result);
    }

    [Fact]
    public async Task HasActiveGameWithFriendAsync_FinishedGame_ReturnsFalse()
    {
        using var context = CreateDbContext("HasActiveGameWithFriend_Finished");
        var repo = new BattleshipGameRepository(context);

        context.BattleshipGames.Add(CreateGame(1, "p1", "p2", GameStatus.Finished));
        await context.SaveChangesAsync();

        var result = await repo.HasActiveGameWithFriendAsync("p1", "p2");
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsNonFinishedGames()
    {
        using var context = CreateDbContext("GetAllActiveAsync");
        var repo = new BattleshipGameRepository(context);

        context.BattleshipGames.Add(CreateGame(1, "p1", "p2", GameStatus.InProgress));
        context.BattleshipGames.Add(CreateGame(2, "p3", "p4", GameStatus.PlacingShips));
        context.BattleshipGames.Add(CreateGame(3, "p5", "p6", GameStatus.Finished));
        await context.SaveChangesAsync();

        var active = await repo.GetAllActiveAsync();
        Assert.Equal(2, active.Count());
    }

    [Fact]
    public async Task GetByPlayerAsync_ReturnsAllPlayerGames()
    {
        using var context = CreateDbContext("GetByPlayerAsync");
        var repo = new BattleshipGameRepository(context);

        context.BattleshipGames.Add(CreateGame(1, "p1", "p2", GameStatus.InProgress));
        context.BattleshipGames.Add(CreateGame(2, "p3", "p1", GameStatus.Finished));
        context.BattleshipGames.Add(CreateGame(3, "p4", "p5", GameStatus.InProgress));
        await context.SaveChangesAsync();

        var games = await repo.GetByPlayerAsync("p1");
        Assert.Equal(2, games.Count());
    }
}
