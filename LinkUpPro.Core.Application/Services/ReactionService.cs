using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enum;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Core.Application.Services
{
    public class ReactionService(IReactionRepository reactionRepository, INotificationService notificationService,
        IPostRepository postRepository, IUserService userService, IFriendshipRepository friendshipRepo) : IReactionService
    {
        private readonly IReactionRepository _reactionRepository = reactionRepository;
        private readonly INotificationService _notificationService = notificationService;
        private readonly IPostRepository _postRepository = postRepository;
        private readonly IUserService _userService = userService;
        private readonly IFriendshipRepository _friendshipRepo = friendshipRepo;

        public async Task<ServiceResult> ReactAsync(int postId, string userId, bool isLike)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null) return ServiceResult.Failure("Publicación no encontrada.");

            if (post.UserId != userId)
            {
                if (post.Privacy == Core.Domain.Enum.PostPrivacy.OnlyMe)
                {
                    return ServiceResult.Failure("No estás autorizado para interactuar con esta publicación.");
                }
                
                if (post.Privacy == Core.Domain.Enum.PostPrivacy.FriendsOnly)
                {
                    bool areFriends = await _friendshipRepo.AreFriendsAsync(userId, post.UserId!);
                    if (!areFriends)
                    {
                        return ServiceResult.Failure("No estás autorizado para interactuar con esta publicación.");
                    }
                }
            }

            var existing = await _reactionRepository.GetByPostAndUserAsync(postId, userId);
            bool isNewReaction = false;

            if (existing == null)
            {
                await _reactionRepository.AddAsync(new Reaction 
                {
                    PostId = postId,
                    UserId = userId,
                    IsLike = isLike
                });
                isNewReaction = true;
            }
            else if(existing.IsLike == isLike)
            {
                await _reactionRepository.DeleteAsync(existing);
                return ServiceResult.Success();
            }
            else
            {
                existing.IsLike = isLike;
                await _reactionRepository.UpdateAsync(existing);
                isNewReaction = true;
            }

            if (isNewReaction)
            {
                if (post != null && post.UserId != userId)
                {
                    var reactorInfo = await _userService.GetUserBasicInfoAsync(userId);
                    string action = isLike ? "reaccionó con 'Me gusta' a" : "reaccionó con 'No me gusta' a";
                    await _notificationService.CreateNotificationAsync(
                        post.UserId!,
                        "Nueva reacción",
                        $"{reactorInfo.Username} {action} tu publicación.",
                        $"/Post/Details/{post.Id}", 
                        NotificationType.PostReaction
                    );
                }
            }

            return ServiceResult.Success();
        }
    }
}
