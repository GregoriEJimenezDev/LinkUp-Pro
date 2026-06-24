using Microsoft.EntityFrameworkCore;
using LinkUpPro.Infrastructure.Persistence.Repositories.Generic;
using LinkUpPro.Infrastructure.Persistence.Context;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class ShipRepository(LinkUpProDbContext context) : GenericRepository<Ship>(context), IShipRepository
    {
        public async Task<IEnumerable<Ship>> GetByGameAndPlayerAsync(int gameId, string playerId) =>
            await _context.Ships.Where(s => s.GameId == gameId && s.PlayerId == playerId)
            .Include(s => s.Cells)
            .AsNoTracking()
            .ToListAsync();
        public async Task<Ship> GetWithCellsAsync(int shipId) =>
            await _context.Ships.Include(s => s.Cells)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == shipId) ?? throw new KeyNotFoundException($"The id {shipId} was not found.");

        public async Task<IEnumerable<Ship>> GetWithCellsByGameAndPlayerAsync(int gameId, string playerId) =>
            await _context.Ships
                .Where(s => s.GameId == gameId && s.PlayerId == playerId)
                .Include(s => s.Cells).AsNoTracking()
                .ToListAsync();

        public async Task<bool> PlayerFinishedPlacingAsync(int gameId, string playerId)
        {
            var count = await _context.Ships
                .CountAsync(s => s.GameId == gameId && s.PlayerId == playerId);
            return count == 5;
        }

        public async Task<int> CountByGameAndPlayerAsync(int gameId, string playerId) =>
            await _context.Ships.CountAsync(s => s.GameId == gameId && s.PlayerId == playerId);
    }
}
