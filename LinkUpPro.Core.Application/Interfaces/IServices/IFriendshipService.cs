using LinkUpPro.Core.Application.DTOs.Friend;
using LinkUpPro.Core.Application.Interfaces.Services;

namespace LinkUpPro.Core.Application.Interfaces.IServices
{
    public interface IFriendshipService
    {
        Task<List<FriendDto>> GetFriendsAsync(string userId);
        Task<List<string>> GetFriendIdsAsync(string userId);
        Task<int> GetMutualFriendsCountAsync(string currentUserId, string targetUserId);
        Task<ServiceResult> RemoveAsync(int friendshipId, string userId);
    }
}
