using Microsoft.EntityFrameworkCore;
using LinkUpPro.Infrastructure.Persistence.Repositories.Generic;
using LinkUpPro.Infrastructure.Persistence.Context;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class FriendRequestRepository(LinkUpProDbContext context) : GenericRepository<FriendRequest>(context), IFriendRequestRepository
    {
        public async Task<IEnumerable<FriendRequest>> GetReceivedByUserAsync(string userId) =>
            await _context.FriendRequests.Where(f => f.ReceiverId == userId && f.Status == FriendRequestStatus.Pending)
            .OrderByDescending(f => f.SentAt)
            .AsNoTracking()
            .ToListAsync();

        public async Task<IEnumerable<FriendRequest>> GetSentByUserAsync(string userId) =>
            await _context.FriendRequests
            .Where(f => f.SenderId == userId)
            .OrderByDescending(f => f.SentAt)
            .AsNoTracking()
            .ToListAsync();

        public async Task<FriendRequest> GetBySenderAndReceiverAsync(string senderId, string receiverId) =>
            await _context.FriendRequests.AsNoTracking().FirstOrDefaultAsync(f => f.SenderId == senderId && f.ReceiverId == receiverId)
            ?? throw new Exception($"Sender and Receiver not found - friendRequestRepository");

        public async Task<bool> HasPendingRequestAsync(string senderId, string receiverId) =>
            await _context.FriendRequests.AnyAsync(f =>
            f.Status == FriendRequestStatus.Pending &&
            ((f.SenderId == senderId && f.ReceiverId == receiverId) ||
            (f.SenderId == receiverId && f.ReceiverId == senderId)));
    }
}
