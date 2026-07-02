using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Users;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class ProfileController(IUserService userService, IPostService postService, IFriendshipService friendshipService) : BaseController
    {
        private readonly IUserService _userService = userService;
        private readonly IPostService _postService = postService;
        private readonly IFriendshipService _friendshipService = friendshipService;

        public async Task<IActionResult> Index(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = await _userService.GetUserIdByUsernameAsync(id);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            var currentUserId = UserId;
            var userInfo = await _userService.GetUserBasicInfoAsync(userId);
            var friends = await _friendshipService.GetFriendIdsAsync(userId);
            var posts = await _postService.GetByUserAsync(userId, currentUserId);

            var currentUserInfo = await _userService.GetUserBasicInfoAsync(currentUserId);
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.ProfilePic = currentUserInfo?.ProfilePictureUrl;

            var vm = new UserProfileViewModel
            {
                UserInfo = userInfo,
                FriendsCount = friends.Count,
                PostsCount = posts.Count,
                Posts = posts.OrderByDescending(p => p.CreatedAt).ToList(),
                IsFriend = friends.Contains(currentUserId) || currentUserId == userId
            };

            return View(vm);
        }

        public async Task<IActionResult> Friends(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Index", "Home");

            var targetUserId = await _userService.GetUserIdByUsernameAsync(id);
            if (string.IsNullOrEmpty(targetUserId))
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            var targetUserInfo = await _userService.GetUserBasicInfoAsync(targetUserId);
            var friends = await _friendshipService.GetFriendsAsync(targetUserId);

            var vm = new ProfileFriendsViewModel
            {
                UserInfo = targetUserInfo,
                Friends = friends,
                IsCurrentUser = targetUserId == UserId
            };

            return View(vm);
        }
    }
}
