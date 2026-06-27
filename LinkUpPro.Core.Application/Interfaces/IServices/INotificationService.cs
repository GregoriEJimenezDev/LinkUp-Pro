using LinkUpPro.Core.Application.DTOs.Notification;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Application.Interfaces.IServices
{
    public interface INotificationService
    {
        Task<ServiceResult> CreateNotificationAsync(string userId, string title, string message, string? url = null, NotificationType type = NotificationType.FriendRequestReceived);
        Task<int> GetUnreadCountAsync(string userId);
        Task<ServiceResult> MarkAsReadAsync(int notificationId, string userId);
        Task<List<NotificationDto>> GetNotificationsAsync(string userId);
    }
}
