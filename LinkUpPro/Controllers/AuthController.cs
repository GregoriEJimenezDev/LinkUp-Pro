using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Users;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("Login", new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _userService.LoginAsync(vm);

            if (result != null && !result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View(vm);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                vm.ProfilePicture = ms.ToArray();
                vm.ProfilePictureFileName = file.FileName;
            }

            var result = await _userService.RegisterAsync(vm);

            if (result != null && !result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View(vm);
            }

            TempData["Success"] = "Cuenta creada exitosamente. Te hemos enviado un correo con el enlace de activación.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ActivateAccount(string token, string userId)
        {
            var result = await _userService.ActivateAccountAsync(token, userId);

            if (result != null && result.Succeeded)
            {
                TempData["Success"] = "Tu cuenta ha sido activada exitosamente. Ya puedes iniciar sesión.";
            }
            else
            {
                TempData["Error"] = result?.ErrorMessage ?? "Ha ocurrido un error al activar tu cuenta.";
            }

            return RedirectToAction("Index");
        }

        public IActionResult ForgotPassword()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string username)
        {
            await _userService.ForgotPasswordAsync(username);

            TempData["Success"] = "Si el usuario existe, se ha enviado un enlace de recuperación al correo asociado.";

            return View();
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var vm = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _userService.ResetPasswordAsync(vm);

            if (result != null && !result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View(vm);
            }

            TempData["Success"] = "Tu contraseña ha sido restablecida exitosamente. Ya puedes iniciar sesión.";
            return RedirectToAction("Index");
        }
    }
}
