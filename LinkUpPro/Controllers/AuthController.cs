using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LinkUpPro.Controllers
{
    [AllowAnonymous]
    public class AuthController(IUserService userService) : BaseController
    {
        private readonly IUserService _userService = userService;

        public IActionResult Index([FromQuery] string? ReturnUrl)
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

            return RedirectToAction("RegisterSuccess");
        }

        public IActionResult RegisterSuccess()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult ResendActivation()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendActivation(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                ModelState.AddModelError("", "Por favor ingresa tu nombre de usuario o correo electrónico.");
                return View();
            }

            var result = await _userService.ResendActivationEmailAsync(username);

            if (result != null && !result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View();
            }

            TempData["Success"] = "Se ha reenviado el enlace de activación. Por favor verifica tu bandeja de entrada o spam.";
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
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            await _userService.ForgotPasswordAsync(vm.Username);

            TempData["Success"] = "Si el usuario existe, se ha enviado un enlace de recuperación al correo asociado.";

            return View(vm);
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

