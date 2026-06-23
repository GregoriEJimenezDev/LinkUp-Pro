using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModel.Save;

namespace LinkUpPro.Core.Application.Interfaces.IServices
{
    public interface ICommentService
    {
        Task<ServiceResult> AddAsync(SaveCommentViewModel vm, string userId);
        Task<ServiceResult> UpdateAsync(SaveCommentViewModel vm, string userId);
        Task<ServiceResult> DeleteAsync(int commentId, string userId);
    }
}
