using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class ReactionController(IReactionService reactionService, IUserService userService, 
        IReactionRepository reactionRepository, IMemoryCache cache) : BaseController
    {
        private readonly IReactionService _reactionService = reactionService;
        private readonly IUserService _userService = userService;
        private readonly IReactionRepository _reactionRepository = reactionRepository;
        private readonly IMemoryCache _cache = cache;

        [HttpPost]
        public async Task<IActionResult> Toggle(int postId, bool isLike)
        {
            var userId = UserId;
            var result = await _reactionService.ReactAsync(postId, userId, isLike);
            if (!result.Succeeded)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            _cache.Remove($"FeedPosts_{userId}_True");
            _cache.Remove($"FeedPosts_{userId}_False");

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

