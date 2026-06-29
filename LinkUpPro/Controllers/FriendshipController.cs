using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Friend;
using System.Security.Claims;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class FriendshipController : BaseController
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IPostService _postService;
        private readonly IUserService _userService;

        public FriendshipController(
            IFriendshipService friendshipService,
            IPostService postService,
            IUserService userService)
        {
            _friendshipService = friendshipService;
            _postService = postService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            
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
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var result = await _friendshipService.RemoveAsync(id, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

