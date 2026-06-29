
using LinkUpPro.Core.Application.Services;

namespace LinkUpPro.Core.Application.Interfaces.IServices
{
    public interface IReactionService
    {
        Task<ServiceResult> ReactAsync(int postId, string userId, bool isLike);
    }
}
