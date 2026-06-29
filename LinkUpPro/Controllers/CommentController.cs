using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Save;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class CommentController : BaseController
    {
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;
        private readonly IMemoryCache _cache;

        public CommentController(ICommentService commentService, IUserService userService, IMemoryCache cache)
        {
            _commentService = commentService;
            _userService = userService;
            _cache = cache;
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveCommentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            var userId = UserId;
            var result = await _commentService.AddAsync(vm, userId);

            if (!result.Succeeded)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            _cache.Remove($"FeedPosts_{userId}");
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveCommentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            var userId = UserId;
            var result = await _commentService.UpdateAsync(vm, userId);

            if (!result.Succeeded)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            _cache.Remove($"FeedPosts_{userId}");
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int postId)
        {
            var userId = UserId;
            var result = await _commentService.DeleteAsync(id, userId);

            if (!result.Succeeded)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            _cache.Remove($"FeedPosts_{userId}");
            return Json(new { success = true });
        }
    }
}

