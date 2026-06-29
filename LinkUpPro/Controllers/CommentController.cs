using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Save;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;

        public CommentController(ICommentService commentService, IUserService userService)
        {
            _commentService = commentService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveCommentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var result = await _commentService.AddAsync(vm, userId);

            if (!result.Succeeded)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveCommentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var result = await _commentService.UpdateAsync(vm, userId);

            if (!result.Succeeded)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int postId)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var result = await _commentService.DeleteAsync(id, userId);

            if (!result.Succeeded)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            return Json(new { success = true });
        }
    }
}
