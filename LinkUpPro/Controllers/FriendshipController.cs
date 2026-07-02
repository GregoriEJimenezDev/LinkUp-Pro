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

        public async Task<IActionResult> Index(string? search, int? mediaType, DateTime? date, bool? isEdited)
        {
            var userId = UserId;
            
            var posts = await _postService.GetByFriendsAsync(userId, includeGlobalPublic: false, includeSelf: false);

            if (!string.IsNullOrWhiteSpace(search))
            {
                posts = posts
                    .Where(p => p.Content.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            LinkUpPro.Core.Domain.Enum.MediaType? filterMediaType = null;
            if (mediaType.HasValue && System.Enum.IsDefined(typeof(LinkUpPro.Core.Domain.Enum.MediaType), mediaType.Value))
            {
                filterMediaType = (LinkUpPro.Core.Domain.Enum.MediaType)mediaType.Value;
                posts = posts.Where(p => p.MediaType == filterMediaType.Value).ToList();
            }

            if (date.HasValue)
            {
                posts = posts.Where(p => p.CreatedAt.Date == date.Value.Date).ToList();
            }

            if (isEdited.HasValue && isEdited.Value)
            {
                posts = posts.Where(p => p.UpdatedAt.HasValue).ToList();
            }

            var vm = new FriendIndexViewModel
            {
                Friends = await _friendshipService.GetFriendsAsync(userId),
                FriendsPosts = posts
            };

            ViewBag.SearchTerm = search;
            ViewBag.FilterMediaType = mediaType;
            ViewBag.FilterDate = date?.ToString("yyyy-MM-dd");
            ViewBag.FilterIsEdited = isEdited;

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

