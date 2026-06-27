using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces.IGeneric;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IFriendshipRepository : IGenericRepository<Friendship>
    {
        Task<IEnumerable<Friendship>> GetFriendsByUserIdAsync(string userId);
        Task<Friendship?> GetByUsersAsync(string userId1, string userId2);
        Task<bool> AreFriendsAsync(string userId1, string userId2);
    }
}
