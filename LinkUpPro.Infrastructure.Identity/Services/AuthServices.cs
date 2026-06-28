using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModel.Users;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace LinkUpPro.Infrastructure.Identity.Services
{
    public class AuthServices(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, 
        IEmailService emailService, IHttpContextAccessor httpContextAccessor, IFileStorageService fileStorageService) : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IEmailService _emailService = emailService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IFileStorageService _fileStorageService = fileStorageService;

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext!.Request;
            return $"{request.Scheme}://{request.Host}";
        }

        public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordViewModel vm)
        {
            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user == null)
                return ServiceResult.Failure("Usuario no encontrado.");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(vm.Token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, vm.Password);

            if (!result.Succeeded)
                return ServiceResult.Failure(result.Errors.First().Description);

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ActivateAccountAsync(string token, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResult.Failure("Usuario no encontrado.");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                return ServiceResult.Failure("Token inválido.");

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ForgotPasswordAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return ServiceResult.Failure("Usuario no encontrado.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var baseUrl = GetBaseUrl();
            var resetLink = $"{baseUrl}/Auth/ResetPassword?userId={user.Id}&token={encodedToken}";

            await _emailService.SendEmailAsync(new EmailRequest
            {
                To = user.Email!,
                Subject = "Recuperación de contraseña - LinkUp",
                Body = $@"
                <h2>¡Hola {user.FirstName}!</h2>
                <p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p>
                <a href='{resetLink}'>Restablecer contraseña</a>
                <p>Si no solicitaste esto, ignora este correo.</p>"
            });

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ResendActivationEmailAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
            
            if (user == null)
                return ServiceResult.Failure("No existe ninguna cuenta asociada a este correo o nombre de usuario.");

            if (user.IsActive)
                return ServiceResult.Failure("Esta cuenta ya se encuentra activa. Puedes iniciar sesión.");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var baseUrl = GetBaseUrl();
            var activationLink = $"{baseUrl}/Auth/ActivateAccount?token={encodedToken}&userId={user.Id}";

            await _emailService.SendEmailAsync(new EmailRequest
            {
                To = user.Email!,
                Subject = "Activa tu cuenta de LinkUp",
                Body = $@"
                <h2>¡Hola {user.FirstName}!</h2>
                <p>Haz clic en el siguiente enlace para activar tu cuenta de LinkUp:</p>
                <a href='{activationLink}'>Activar cuenta</a>
                <p>Si no puedes hacer clic, copia este enlace en tu navegador:</p>
                <p>{activationLink}</p>"
            });

            return ServiceResult.Success();
        }

        public async Task<EditProfileViewModel> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                return new EditProfileViewModel(); 
            }
            
            return new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                CurrentProfilePicture = user.ProfilePicturePath
            };
        }

        public async Task<ServiceResult> LoginAsync(LoginViewModel vm)
        {
            var user = await _userManager.FindByNameAsync(vm.Username);

            if (user == null)
                return ServiceResult.Failure("El nombre de usuario o la contraseña son incorrectos.");

            if (await _userManager.IsLockedOutAsync(user))
                return ServiceResult.Failure("La cuenta se encuentra bloqueada temporalmente debido a varios intentos fallidos. Intenta de nuevo en 15 minutos.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, vm.Password!);
            if (!passwordValid)
            {
                await _userManager.AccessFailedAsync(user);
                
                if (await _userManager.IsLockedOutAsync(user))
                    return ServiceResult.Failure("La cuenta se encuentra bloqueada temporalmente debido a varios intentos fallidos. Intenta de nuevo en 15 minutos.");
                    
                return ServiceResult.Failure("El nombre de usuario o la contraseña son incorrectos.");
            }

            if (!user.IsActive)
                return ServiceResult.Failure("Su cuenta se encuentra inactiva. Debe activarla mediante el enlace enviado a su correo electrónico.");

            await _userManager.ResetAccessFailedCountAsync(user);

            await _signInManager.SignInAsync(user, isPersistent: vm.RememberMe);

            return ServiceResult.Success();
        }

        public async Task LogoutAsync() => await _signInManager.SignOutAsync();

        public async Task<ServiceResult> RegisterAsync(RegisterViewModel vm)
        {
            var userExisting = await _userManager.FindByNameAsync(vm.Username);
            if (userExisting != null)
                return ServiceResult.Failure("Este nombre de usuario ya existe.");

            var existingEmail = await _userManager.FindByEmailAsync(vm.Email);
            if (existingEmail != null)
                return ServiceResult.Failure("Este correo electrónico ya está registrado.");

            var user = new ApplicationUser
            {
                UserName = vm.Username,
                Email = vm.Email,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                IsActive = false
            };

            user.Phone = Regex.Replace(vm.PhoneNumber, @"[^\d]", "");

            if (vm.ProfilePicture != null && vm.ProfilePicture.Length > 0)
            {
                if (vm.ProfilePicture.Length > 5 * 1024 * 1024)
                    return ServiceResult.Failure("La imagen de perfil no puede superar los 5 MB.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(vm.ProfilePictureFileName ?? "").ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return ServiceResult.Failure("Solo se permiten imágenes .jpg, .png y .webp.");

                var fileName = $"{Guid.NewGuid()}{extension}";
                var contentType = extension == ".png" ? "image/png" : (extension == ".webp" ? "image/webp" : "image/jpeg");
                
                user.ProfilePicturePath = await _fileStorageService.UploadFileAsync(vm.ProfilePicture, fileName, "profiles", contentType);
            }

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
                return ServiceResult.Failure(result.Errors.First().Description);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var baseUrl = GetBaseUrl();
            var activationLink = $"{baseUrl}/Auth/ActivateAccount?token={encodedToken}&userId={user.Id}";

            await _emailService.SendEmailAsync(new EmailRequest
            {
                To = user.Email,
                Subject = "Activa tu cuenta de LinkUp",
                Body = $@"
                <h2>¡Bienvenido a LinkUp, {user.FirstName}!</h2>
                <p>Haz clic en el siguiente enlace para activar tu cuenta:</p>
                <a href='{activationLink}'>Activar cuenta</a>
                <p>Si no puedes hacer clic, copia este enlace en tu navegador:</p>
                <p>{activationLink}</p>"
            });

            return ServiceResult.Success();
        }

        public async Task<string> GetUserIdByUsernameAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user?.Id ?? string.Empty;
        }

        public async Task<UserBasicDto> GetUserBasicInfoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new UserBasicDto();

            return new UserBasicDto
            {
                Id = user.Id,
                Username = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePicturePath,
                Email = user.Email!,
                IsActive = user.IsActive
            };
        }

        public async Task<ServiceResult> UpdateProfileAsync(EditProfileViewModel vm, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResult.Failure("Usuario no encontrado.");

            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.Phone = Regex.Replace(vm.Phone, @"[^\d]", "");

            if (vm.ProfilePicture != null && vm.ProfilePicture.Length > 0)
            {
                if (vm.ProfilePicture.Length > 5 * 1024 * 1024)
                    return ServiceResult.Failure("La imagen de perfil no puede superar los 5 MB.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(vm.ProfilePictureFileName ?? "").ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return ServiceResult.Failure("Solo se permiten imágenes .jpg, .png y .webp.");

                var fileName = $"{Guid.NewGuid()}{extension}";
                var contentType = extension == ".png" ? "image/png" : (extension == ".webp" ? "image/webp" : "image/jpeg");
                
                user.ProfilePicturePath = await _fileStorageService.UploadFileAsync(vm.ProfilePicture, fileName, "profiles", contentType);
            }

            if (!string.IsNullOrEmpty(vm.Password) && !string.IsNullOrEmpty(vm.CurrentPassword))
            {
                var checkPassword = await _userManager.CheckPasswordAsync(user, vm.CurrentPassword);
                if (!checkPassword)
                    return ServiceResult.Failure("La contraseña actual es incorrecta.");

                var result = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.Password);
                if (!result.Succeeded)
                    return ServiceResult.Failure(result.Errors.First().Description);

                await _userManager.UpdateSecurityStampAsync(user);
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return ServiceResult.Failure(updateResult.Errors.First().Description);

            return ServiceResult.Success();
        }

        public async Task<IEnumerable<UserBasicDto>> GetAllActiveUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            var activeUsers = users
                .Where(u => u.IsActive)
                .Select(user => new UserBasicDto
                {
                    Id = user.Id,
                    Username = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfilePictureUrl = user.ProfilePicturePath,
                    Email = user.Email!,
                    IsActive = user.IsActive
                })
                .ToList();

            return activeUsers;
        }

    }
}
