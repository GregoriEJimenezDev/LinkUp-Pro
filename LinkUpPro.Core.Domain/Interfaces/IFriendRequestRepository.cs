using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces.IGeneric;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IFriendRequestRepository : IGenericRepository<FriendRequest>
    {
        Task<IEnumerable<FriendRequest>> GetReceivedByUserAsync(string userId);
        Task<IEnumerable<FriendRequest>> GetSentByUserAsync(string userId);
        Task<FriendRequest> GetBySenderAndReceiverAsync(string senderId, string receiverId);
        Task<bool> HasPendingRequestAsync(string senderId, string receiverId);
    }
}
