using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Friend;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class FriendshipController(
        IFriendshipService friendshipService,
        IPostService postService,
        IUserService userService) : BaseController
    {
        private readonly IFriendshipService _friendshipService = friendshipService;
        private readonly IPostService _postService = postService;
        private readonly IUserService _userService = userService;

        public async Task<IActionResult> Index()
        {
            var userId = UserId;
            
            var vm = new FriendIndexViewModel
            {
                Friends = await _friendshipService.GetFriendsAsync(userId),
                FriendsPosts = await _postService.GetByFriendsAsync(userId)
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = UserId;
            var result = await _friendshipService.RemoveAsync(id, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

