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

        public async Task<IActionResult> Index(string? search, int? mediaType, DateTime? dateFrom, DateTime? dateTo, int? editState, string? friendUsername)
        {
            var userId = UserId;
            ViewBag.CurrentUserId = userId;
            
            var posts = await _postService.GetByFriendsAsync(userId, includeGlobalPublic: false, includeSelf: false);

            if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value.Date > dateTo.Value.Date)
            {
                TempData["Error"] = "La fecha inicial no puede ser posterior a la fecha final.";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(search))
                {
                    posts = posts
                        .Where(p => p.Content.Contains(search, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(friendUsername))
                {
                    posts = posts
                        .Where(p => p.Username != null && p.Username.Equals(friendUsername, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                LinkUpPro.Core.Domain.Enum.MediaType? filterMediaType = null;
                if (mediaType.HasValue && System.Enum.IsDefined(typeof(LinkUpPro.Core.Domain.Enum.MediaType), mediaType.Value))
                {
                    filterMediaType = (LinkUpPro.Core.Domain.Enum.MediaType)mediaType.Value;
                    posts = posts.Where(p => p.MediaType == filterMediaType.Value).ToList();
                }

                if (dateFrom.HasValue)
                {
                    posts = posts.Where(p => p.CreatedAt.Date >= dateFrom.Value.Date).ToList();
                }

                if (dateTo.HasValue)
                {
                    posts = posts.Where(p => p.CreatedAt.Date <= dateTo.Value.Date).ToList();
                }

                if (editState.HasValue)
                {
                    if (editState.Value == 1) // Editadas
                    {
                        posts = posts.Where(p => p.UpdatedAt.HasValue).ToList();
                    }
                    else if (editState.Value == 2) // No editadas
                    {
                        posts = posts.Where(p => !p.UpdatedAt.HasValue).ToList();
                    }
                }
            }

            var vm = new FriendIndexViewModel
            {
                Friends = await _friendshipService.GetFriendsAsync(userId),
                FriendsPosts = posts,
                SearchQuery = search,
                FilterDateFrom = dateFrom,
                FilterDateTo = dateTo,
                FilterEditState = editState,
                FriendUsername = friendUsername
            };
            if (mediaType.HasValue && System.Enum.IsDefined(typeof(LinkUpPro.Core.Domain.Enum.MediaType), mediaType.Value))
            {
                vm.FilterMediaType = (LinkUpPro.Core.Domain.Enum.MediaType)mediaType.Value;
            }

            ViewBag.SearchTerm = search;

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

