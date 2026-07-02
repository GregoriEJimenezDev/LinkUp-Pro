using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Post;
using LinkUpPro.Core.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class HomeController(IPostService postService, IUserService userService, IMemoryCache cache) : BaseController
    {
        private readonly IPostService _postService = postService;
        private readonly IUserService _userService = userService;
        private readonly IMemoryCache _cache = cache;

        public async Task<IActionResult> Index(string? search, int? mediaType, DateTime? date, bool? isEdited)
        {
            var userId = UserId;
            var posts = await _postService.GetByFriendsAsync(userId, includeGlobalPublic: true);

            if (!string.IsNullOrWhiteSpace(search))
            {
                posts = posts
                    .Where(p => p.Content.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            MediaType? filterMediaType = null;
            if (mediaType.HasValue && System.Enum.IsDefined(typeof(MediaType), mediaType.Value))
            {
                filterMediaType = (MediaType)mediaType.Value;
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

            ViewBag.CurrentUserId = userId;

            var userInfo = await _userService.GetUserBasicInfoAsync(userId);

            var vm = new HomeViewModel
            {
                Posts = posts,
                SearchQuery = search,
                FilterMediaType = filterMediaType,
                FilterDate = date,
                FilterIsEdited = isEdited,
                CurrentUserProfilePicture = userInfo.ProfilePictureUrl
            };

            return View(vm);
        }
    }
}

