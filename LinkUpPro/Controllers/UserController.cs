using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Users;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class UserController(IUserService userService) : BaseController
    {
        private readonly IUserService _userService = userService;

        public async Task<IActionResult> Profile()
        {
            var userId = UserId;
            var profile = await _userService.GetProfileAsync(userId);
            var userInfo = await _userService.GetUserBasicInfoAsync(userId);

            ViewBag.Username = User.Identity!.Name;
            ViewBag.Email = userInfo.Email;

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(EditProfileViewModel vm, IFormFile? File)
        {
            if (File != null)
            {
                if (File.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProfilePicture", "La imagen seleccionada no puede superar los 5 MB.");
                }

                var extension = Path.GetExtension(File.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ProfilePicture", "El archivo seleccionado no tiene un formato de imagen válido.");
                }
            }

            if (!ModelState.IsValid)
            {
                var uid = UserId;
                var userInfo = await _userService.GetUserBasicInfoAsync(uid);
                ViewBag.Username = User.Identity!.Name;
                ViewBag.Email = userInfo.Email;
                return View(vm);
            }

            var userId = UserId;

            if (File != null)
            {
                using var memoryStream = new MemoryStream();
                await File.CopyToAsync(memoryStream);
                vm.ProfilePicture = memoryStream.ToArray();
                vm.ProfilePictureFileName = File.FileName;
            }

            var result = await _userService.UpdateProfileAsync(vm, userId);

            if (!result.Succeeded)
            {
                var userInfo = await _userService.GetUserBasicInfoAsync(userId);
                ViewBag.Username = User.Identity!.Name;
                ViewBag.Email = userInfo.Email;
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Error al actualizar perfil");
                return View(vm);
            }
            if (!string.IsNullOrEmpty(vm.Password))
            {
                TempData["Success"] = "Su perfil y contraseña fueron actualizados correctamente. Inicie sesión nuevamente.";
                return RedirectToAction("Logout", "Auth");
            }

            TempData["Success"] = "Su perfil fue actualizado correctamente.";
            return RedirectToAction(nameof(Profile));
        }
    }
}

