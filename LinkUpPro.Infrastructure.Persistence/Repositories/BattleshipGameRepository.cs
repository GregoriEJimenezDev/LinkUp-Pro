using Microsoft.EntityFrameworkCore;
using LinkUpPro.Infrastructure.Persistence.Repositories.Generic;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Infrastructure.Persistence.Context;
using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class BattleshipGameRepository(LinkUpProDbContext context) : GenericRepository<BattleshipGame>(context), IBattleshipGameRepository
    {
        public async Task<IEnumerable<BattleshipGame>> GetActiveByUserIdAsync(string userId) =>
            await _context.BattleshipGames
                .Where(g => (g.FirstPlayerId == userId || g.SecondPlayerId == userId) && g.Status != GameStatus.Finished && g.Status != GameStatus.Abandoned)
                .OrderByDescending(g => g.StartedAt)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<BattleshipGame>> GetFinishedByUserIdAsync(string userId) =>
            await _context.BattleshipGames
                .Where(g => (g.FirstPlayerId == userId || g.SecondPlayerId == userId) && g.Status == GameStatus.Finished)
                .OrderByDescending(g => g.FinishedAt)
                .AsNoTracking()
                .ToListAsync();

        public async Task<BattleshipGame> GetWithDetailsAsync(int gameId) =>
            await _context.BattleshipGames
                .Include(g => g.Ships).ThenInclude(s => s.Cells)
                .Include(g => g.Attacks)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == gameId)
            ?? throw new Exception($"Details not found.");

        public async Task<BattleshipGame> GetWithDetailsForUpdateAsync(int gameId) =>
            await _context.BattleshipGames
                .Include(g => g.Ships).ThenInclude(s => s.Cells)
                .Include(g => g.Attacks)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == gameId)
            ?? throw new Exception($"Details not found.");

        public async Task<bool> HasActiveGameWithFriendAsync(string userId, string friendId) =>
            await _context.BattleshipGames.AsNoTracking()
                .AnyAsync(g => g.Status != GameStatus.Finished && g.Status != GameStatus.Abandoned && ((g.FirstPlayerId == userId && g.SecondPlayerId == friendId) || 
                (g.FirstPlayerId == friendId && g.SecondPlayerId == userId)));

        public async Task<IEnumerable<BattleshipGame>> GetAllActiveAsync() =>
            await _context.BattleshipGames
                .Where(g => g.Status != GameStatus.Finished && g.Status != GameStatus.Abandoned)
                .ToListAsync();

        public async Task<IEnumerable<BattleshipGame>> GetByPlayerAsync(string userId) =>
            await _context.BattleshipGames
                .Where(g => g.FirstPlayerId == userId || g.SecondPlayerId == userId)
                .OrderByDescending(g => g.StartedAt)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<BattleshipGame>> GetAllFinishedAsync() =>
            await _context.BattleshipGames
                .Where(g => g.Status == GameStatus.Finished)
                .OrderByDescending(g => g.FinishedAt)
                .AsNoTracking()
                .ToListAsync();
    }
}
