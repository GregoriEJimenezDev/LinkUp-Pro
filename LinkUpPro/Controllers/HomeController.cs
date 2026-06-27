using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Post;
using LinkUpPro.Core.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPostService _postService;
        private readonly IUserService _userService;

        public HomeController(IPostService postService, IUserService userService)
        {
            _postService = postService;
            _userService = userService;
        }

        public async Task<IActionResult> Index(string? search, int? mediaType)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var posts = await _postService.GetByFriendsAsync(userId);

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
