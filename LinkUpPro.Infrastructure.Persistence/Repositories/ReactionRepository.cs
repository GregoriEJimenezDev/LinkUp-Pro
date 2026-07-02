using Microsoft.EntityFrameworkCore;
using LinkUpPro.Infrastructure.Persistence.Repositories.Generic;
using LinkUpPro.Infrastructure.Persistence.Context;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class ReactionRepository(LinkUpProDbContext context) : GenericRepository<Reaction>(context), IReactionRepository
    {
        public async Task<Reaction?> GetByPostAndUserAsync(int postId, string userId) =>
            await _context.Reactions.AsNoTracking().FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);

        public async Task<IEnumerable<Reaction>> GetByPostIdAsync(int postId) =>
            await _context.Reactions
            .Where(r => r.PostId == postId)
            .AsNoTracking()
            .ToListAsync();
    }
}
