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

        public async Task<IActionResult> Index(string? search, int? mediaType, DateTime? dateFrom, DateTime? dateTo, int? editState)
        {
            var userId = UserId;
            var posts = await _postService.GetByUserAsync(userId, userId);

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

                MediaType? filterMediaType = null;
                if (mediaType.HasValue && System.Enum.IsDefined(typeof(MediaType), mediaType.Value))
                {
                    filterMediaType = (MediaType)mediaType.Value;
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

            ViewBag.CurrentUserId = userId;

            var userInfo = await _userService.GetUserBasicInfoAsync(userId);

            MediaType? selectedMediaType = null;
            if (mediaType.HasValue && System.Enum.IsDefined(typeof(MediaType), mediaType.Value))
            {
                selectedMediaType = (MediaType)mediaType.Value;
            }

            var vm = new HomeViewModel
            {
                Posts = posts,
                SearchQuery = search,
                FilterMediaType = selectedMediaType,
                FilterDateFrom = dateFrom,
                FilterDateTo = dateTo,
                FilterEditState = editState,
                CurrentUserProfilePicture = userInfo.ProfilePictureUrl
            };

            return View(vm);
        }
    }
}


