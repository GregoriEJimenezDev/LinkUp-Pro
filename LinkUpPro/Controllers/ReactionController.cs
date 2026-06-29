using LinkUpPro.Core.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class ReactionController : BaseController
    {
        private readonly IReactionService _reactionService;
        private readonly IUserService _userService;
        private readonly LinkUpPro.Core.Domain.Interfaces.IReactionRepository _reactionRepository;
        private readonly IMemoryCache _cache;

        public ReactionController(IReactionService reactionService, IUserService userService, LinkUpPro.Core.Domain.Interfaces.IReactionRepository reactionRepository, IMemoryCache cache)
        {
            _reactionService = reactionService;
            _userService = userService;
            _reactionRepository = reactionRepository;
            _cache = cache;
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int postId, bool isLike)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            await _reactionService.ReactAsync(postId, userId, isLike);

            _cache.Remove($"FeedPosts_{userId}");

            var reactions = await _reactionRepository.GetByPostIdAsync(postId);
            var likesCount = reactions.Count(r => r.IsLike);
            var dislikesCount = reactions.Count(r => !r.IsLike);
            var userReaction = reactions.FirstOrDefault(r => r.UserId == userId);

            return Json(new { 
                success = true, 
                likesCount = likesCount, 
                dislikesCount = dislikesCount, 
                currentUserReaction = userReaction?.IsLike 
            });
        }
    }
}

