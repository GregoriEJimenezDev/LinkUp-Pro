using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Save;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class PostController(IPostService postService, IUserService userService, IMemoryCache cache) : BaseController
    {
        private readonly IPostService _postService = postService;
        private readonly IUserService _userService = userService;
        private readonly IMemoryCache _cache = cache;

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
                vm.ImageFileName = File.FileName;
            }

            var userId = UserId;
            var result = await _postService.CreateAsync(vm, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Error al crear: " + result.ErrorMessage;
            }
            else 
            {
                _cache.Remove($"FeedPosts_{userId}_True");
                _cache.Remove($"FeedPosts_{userId}_False");
                TempData["Success"] = "¡Publicación creada exitosamente!";
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = UserId;
            var vm = await _postService.GetByIdForEditAsync(id, userId);
            
            if (vm == null)
            {
                TempData["Error"] = "No se encontró la publicación o no estás autorizado para editarla.";
                return RedirectToAction("Index", "Home");
            }
            
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var exists = await _postService.IsPostAvailableAsync(id);
            if (!exists)
            {
                TempData["Error"] = "Esta publicación ya no se encuentra disponible.";
                return RedirectToAction("Index", "Home");
            }
            
            return RedirectToAction("Index", "Home", null, $"post-{id}");
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
                vm.ImageFileName = File.FileName;
            }

            var userId = UserId;
            var result = await _postService.UpdateAsync(vm, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
                return View(vm);
            }

            _cache.Remove($"FeedPosts_{userId}_True");
            _cache.Remove($"FeedPosts_{userId}_False");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = UserId;
            var result = await _postService.DeleteAsync(id, userId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.ErrorMessage;
            }
            else
            {
                _cache.Remove($"FeedPosts_{userId}_True");
                _cache.Remove($"FeedPosts_{userId}_False");
                TempData["Success"] = "La publicación fue eliminada correctamente.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
