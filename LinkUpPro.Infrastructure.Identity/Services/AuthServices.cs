using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.Services;
using LinkUpPro.Core.Application.ViewModel.Users;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Caching.Memory;

namespace LinkUpPro.Infrastructure.Identity.Services
{
    public class AuthServices(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, 
        IEmailService emailService, IHttpContextAccessor httpContextAccessor, IFileStorageService fileStorageService,
        IMemoryCache cache) : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IEmailService _emailService = emailService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly IMemoryCache _cache = cache;

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

            await _userManager.UpdateSecurityStampAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);

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
            var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
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
            
            if (user == null || user.IsActive)
                return ServiceResult.Success();

            var cacheKey = $"ResendEmail_{user.Id}";
            if (_cache.TryGetValue(cacheKey, out _))
            {
                return ServiceResult.Success();
            }

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

            _cache.Set(cacheKey, true, TimeSpan.FromMinutes(5));

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

                if (!IsValidImageFile(vm.ProfilePicture))
                    return ServiceResult.Failure("El archivo no es una imagen válida o está corrupto.");

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

        public async Task<Dictionary<string, UserBasicDto>> GetUsersBasicInfoAsync(IEnumerable<string> userIds)
        {
            var distinctIds = userIds.Distinct().ToList();
            if (!distinctIds.Any()) return new Dictionary<string, UserBasicDto>();

            var users = await _userManager.Users
                .Where(u => distinctIds.Contains(u.Id))
                .ToListAsync();

            return users.ToDictionary(
                u => u.Id,
                u => new UserBasicDto
                {
                    Id = u.Id,
                    Username = u.UserName!,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    ProfilePictureUrl = u.ProfilePicturePath,
                    Email = u.Email!,
                    IsActive = u.IsActive
                }
            );
        }

        public async Task<UserBasicDto> GetUserBasicInfoAsync(string userId)
        {
            var cacheKey = $"UserInfo_{userId}";
            if (_cache.TryGetValue(cacheKey, out UserBasicDto? cachedInfo) && cachedInfo != null)
            {
                return cachedInfo;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new UserBasicDto();

            var info = new UserBasicDto
            {
                Id = user.Id,
                Username = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePicturePath,
                Email = user.Email!,
                IsActive = user.IsActive
            };

            _cache.Set(cacheKey, info, TimeSpan.FromMinutes(10));
            return info;
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

                if (!IsValidImageFile(vm.ProfilePicture))
                    return ServiceResult.Failure("El archivo no es una imagen válida o está corrupto.");

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

            // Invalidate cache
            _cache.Remove($"UserInfo_{userId}");

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


        private static bool IsValidImageFile(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length < 4) return false;

            var header = fileBytes.Take(4).ToArray();

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;

            // PNG: 89 50 4E 47
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;

            // WEBP: RIFF ... WEBP
            if (fileBytes.Length > 12)
            {
                var webpHeader = fileBytes.Take(12).ToArray();
                if (webpHeader[0] == 0x52 && webpHeader[1] == 0x49 && webpHeader[2] == 0x46 && webpHeader[3] == 0x46 &&
                    webpHeader[8] == 0x57 && webpHeader[9] == 0x45 && webpHeader[10] == 0x42 && webpHeader[11] == 0x50)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
