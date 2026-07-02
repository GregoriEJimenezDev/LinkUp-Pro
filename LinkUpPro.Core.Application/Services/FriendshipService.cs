using LinkUpPro.Core.Application.DTOs.Friend;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Core.Application.Services
{
    public class FriendshipService(IFriendshipRepository friendshipRepository, IUserService userService) : IFriendshipService
    {
        private readonly IFriendshipRepository _friendshipRepository = friendshipRepository;
        private readonly IUserService _userServices = userService;

        public async Task<List<string>> GetFriendIdsAsync(string userId)
        {
            var friendships = await _friendshipRepository.GetFriendsByUserIdAsync(userId);
            return friendships.Select(f => f.FirstUserId == userId ? f.SecondUserId : f.FirstUserId).ToList()!;
        }

        public async Task<int> GetMutualFriendsCountAsync(string currentUserId, string targetUserId)
        {
            var currentUserFriends = await GetFriendIdsAsync(currentUserId);
            var targetUserFriends = await GetFriendIdsAsync(targetUserId);

            return currentUserFriends.Intersect(targetUserFriends).Count();
        }

        public async Task<List<FriendDto>> GetFriendsAsync(string userId)
        {

            var friendships = await _friendshipRepository.GetFriendsByUserIdAsync(userId);
            var result = new List<FriendDto>();

            foreach (var f in friendships)
            {
                var friendId = f.FirstUserId == userId ? f.SecondUserId : f.FirstUserId;
                var info = await _userServices.GetUserBasicInfoAsync(friendId!);
                
                if (string.IsNullOrEmpty(info.Username)) continue;

                result.Add(new FriendDto
                {
                    UserId = friendId!,
                    Username = info.Username,
                    FullName = $"{info.FirstName} {info.LastName}",
                    ProfilePicture = info.ProfilePictureUrl,
                    FriendshipId = f.Id
                });
            }
            return result;
        }
        public async Task<ServiceResult> RemoveAsync(int friendshipId, string userId)
        {
            var friendship = await _friendshipRepository.GetByIdAsync(friendshipId);
            if (friendship == null)
                return ServiceResult.Failure("Relación de amistad no encontrada.");
            if (friendship.FirstUserId != userId && friendship.SecondUserId != userId)
                return ServiceResult.Failure("No formas parte de esta relación de amistad.");

            await _friendshipRepository.DeleteAsync(friendship);
            return ServiceResult.Success();
        }
    }
}
