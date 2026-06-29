using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Post;
using LinkUpPro.Core.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly IMemoryCache _cache;

        public HomeController(IPostService postService, IUserService userService, IMemoryCache cache)
        {
            _postService = postService;
            _userService = userService;
            _cache = cache;
        }

        public async Task<IActionResult> Index(string? search, int? mediaType)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var cacheKey = $"FeedPosts_{userId}";

            if (!_cache.TryGetValue(cacheKey, out List<PostViewModel>? posts) || posts == null)
            {
                posts = await _postService.GetByFriendsAsync(userId);
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
                _cache.Set(cacheKey, posts, cacheOptions);
            }

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

            ViewBag.CurrentUserId = userId;

            var vm = new HomeViewModel
            {
                Posts = posts,
                SearchQuery = search,
                FilterMediaType = filterMediaType
            };

            return View(vm);
        }
    }
}

