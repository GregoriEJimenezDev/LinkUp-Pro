using LinkUpPro.Core.Application.Services;
using LinkUpPro.Core.Application.ViewModel.Post;
using LinkUpPro.Core.Application.ViewModel.Save;

namespace LinkUpPro.Core.Application.Interfaces.IServices
{
    public interface IPostService
    {
        Task<List<PostViewModel>> GetByUserAsync(string targetUserId, string currentUserId);
        Task<List<PostViewModel>> GetByFriendsAsync(string userId, bool includeGlobalPublic = false);
        Task<ServiceResult> CreateAsync(SavePostViewModel vm, string userId);
        Task<ServiceResult> UpdateAsync(SavePostViewModel vm, string userId);
        Task<ServiceResult> DeleteAsync(int postId, string userId);
        Task<SavePostViewModel?> GetByIdForEditAsync(int postId, string userId);
    }
}
