using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Save;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly IUserService _userService;

        public PostController(IPostService postService, IUserService userService)
        {
            _postService = postService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(SavePostViewModel vm, IFormFile? File)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["Error"] = "Error al crear: " + errors;
                return RedirectToAction("Index", "Home");
            }

            if (File != null && File.Length > 0)
            {
                using var ms = new MemoryStream();
                await File.CopyToAsync(ms);
                vm.ImageFile = ms.ToArray();
            }

            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var result = await _postService.CreateAsync(vm, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Error al crear: " + result.ErrorMessage;
            }
            else 
            {
                TempData["Success"] = "¡Publicación creada exitosamente!";
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _postService.GetByIdForEditAsync(id);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SavePostViewModel vm, IFormFile? File)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (File != null && File.Length > 0)
            {
                using var ms = new MemoryStream();
                await File.CopyToAsync(ms);
                vm.ImageFile = ms.ToArray();
            }

            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var result = await _postService.UpdateAsync(vm, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
                return View(vm);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var result = await _postService.DeleteAsync(id, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
