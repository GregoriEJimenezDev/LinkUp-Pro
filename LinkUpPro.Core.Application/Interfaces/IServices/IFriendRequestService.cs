using LinkUpPro.Core.Application.Services;
using LinkUpPro.Core.Application.ViewModel.Friend;

namespace LinkUpPro.Core.Application.Interfaces.IServices
{
    public interface IFriendRequestService
    {
        Task<FriendRequestIndexViewModel> GetRequestsAsync(string userId);
        Task<SendFriendRequestViewModel> GetAvailableUsersAsync(string userId, string? search);
        Task<ServiceResult> SendAsync(string senderId, string receiverId);
        Task<ServiceResult> AcceptAsync(int requestId, string userId);
        Task<ServiceResult> RejectAsync(int requestId, string userId);
        Task<ServiceResult> DeleteAsync(int requestId, string userId);
        Task<ServiceResult> RemoveFromHistoryAsync(int requestId, string userId);
        Task<int> GetPendingCountAsync(string userId);
    }
}
