using Microsoft.EntityFrameworkCore;
using LinkUpPro.Infrastructure.Persistence.Repositories.Generic;
using LinkUpPro.Infrastructure.Persistence.Context;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class FriendshipRepository(LinkUpProDbContext context) : GenericRepository<Friendship>(context), IFriendshipRepository
    {
        public async Task<IEnumerable<Friendship>> GetFriendsByUserIdAsync(string userId) =>
            await _context.Friendships.Where(f => f.FirstUserId == userId || f.SecondUserId == userId).AsNoTracking().ToListAsync();

        public async Task<Friendship> GetByUsersAsync(string userId1, string userId2) =>
            await _context.Friendships.AsNoTracking().FirstOrDefaultAsync(f =>
            (f.FirstUserId == userId1 && f.SecondUserId == userId2) ||
            (f.FirstUserId == userId2 && f.SecondUserId == userId1)) ?? throw new KeyNotFoundException($"Users {userId1} and {userId2} not found.");

        public async Task<bool> AreFriendsAsync(string userId1, string userId2) =>
            await _context.Friendships.AsNoTracking().AnyAsync(f =>
            (f.FirstUserId == userId1 && f.SecondUserId == userId2) || (f.FirstUserId == userId2 && f.SecondUserId == userId1));
    }
}
