using Microsoft.EntityFrameworkCore;
using LinkUpPro.Infrastructure.Persistence.Repositories.Generic;
using LinkUpPro.Infrastructure.Persistence.Context;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class AttackRepository(LinkUpProDbContext context) : GenericRepository<Attack>(context), IAttackRepository
    {
        public async Task<IEnumerable<Attack>> GetByGameAndAttackerAsync(int gameId, string attackerId) =>
            await _context.Attacks.Where(a => a.GameId == gameId && a.AttackerId == attackerId).AsNoTracking().ToListAsync();

        public async Task<bool> CellAlreadyAttackedAsync(int gameId, string attackerId, int row, int col) =>
            await _context.Attacks.AsNoTracking().AnyAsync(a => a.GameId == gameId
            && a.AttackerId == attackerId
            && a.Row == row
            && a.Column == col);
    }
}