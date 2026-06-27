using LinkUpPro.Core.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class ReactionController : Controller
    {
        private readonly IReactionService _reactionService;
        private readonly IUserService _userService;

        public ReactionController(IReactionService reactionService, IUserService userService)
        {
            _reactionService = reactionService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int postId, bool isLike)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            await _reactionService.ReactAsync(postId, userId, isLike);

            return Redirect(Request.Headers.Referer.ToString() ?? "/");
        }
    }
}
