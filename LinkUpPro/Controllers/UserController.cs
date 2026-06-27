using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Users;
using System.Security.Claims;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Profile()
        {
            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
            var profile = await _userService.GetProfileAsync(userId);
            var userInfo = await _userService.GetUserBasicInfoAsync(userId);

            ViewBag.Username = User.Identity!.Name;
            ViewBag.Email = userInfo.Email;

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(EditProfileViewModel vm, IFormFile? File)
        {
            if (!ModelState.IsValid)
            {
                var uid = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);
                var userInfo = await _userService.GetUserBasicInfoAsync(uid);
                ViewBag.Username = User.Identity!.Name;
                ViewBag.Email = userInfo.Email;
                return View(vm);
            }

            var userId = await _userService.GetUserIdByUsernameAsync(User.Identity!.Name!);

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

            TempData["Success"] = "¡Perfil actualizado exitosamente!";
            return RedirectToAction(nameof(Profile));
        }
    }
}
