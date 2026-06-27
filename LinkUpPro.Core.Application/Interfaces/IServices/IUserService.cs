using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModel.Users;

namespace LinkUpPro.Core.Application.Interfaces.IServices
{
    public interface IUserService
    {
        Task<ServiceResult> RegisterAsync(RegisterViewModel vm);
        Task<ServiceResult> LoginAsync(LoginViewModel vm);
        Task LogoutAsync();
        Task<ServiceResult> ActivateAccountAsync(string token, string userId);
        Task<ServiceResult> ForgotPasswordAsync(string username);
        Task<ServiceResult> ResendActivationEmailAsync(string username);
        Task<ServiceResult> ResetPasswordAsync(ResetPasswordViewModel vm);
        Task<ServiceResult> UpdateProfileAsync(EditProfileViewModel vm, string userId);
        Task<UserBasicDto> GetUserBasicInfoAsync(string userId);
        Task<string> GetUserIdByUsernameAsync(string username);
        Task<EditProfileViewModel> GetProfileAsync(string userId);
        Task<IEnumerable<UserBasicDto>> GetAllActiveUsersAsync();
    }
}
