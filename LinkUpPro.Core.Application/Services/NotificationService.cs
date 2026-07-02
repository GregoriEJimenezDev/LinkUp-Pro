using LinkUpPro.Core.Application.DTOs.Notification;
using LinkUpPro.Core.Domain.Enum;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Core.Application.Services
{
    public class NotificationService(INotificationRepository notificationRepo) : INotificationService
    {
        private readonly INotificationRepository _notificationRepo = notificationRepo;

        public async Task<ServiceResult> CreateNotificationAsync(string userId, string title, string message, string? url = null, NotificationType type = NotificationType.FriendRequestReceived)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Url = url,
                Type = type,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepo.AddAsync(notification);
            return ServiceResult.Success();
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepo.GetByUserIdAsync(userId);
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Url = n.Url,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var notifications = await _notificationRepo.GetByUserIdAsync(userId);
            return notifications.Count(n => !n.IsRead);
        }

        public async Task<ServiceResult> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification == null) return ServiceResult.Failure("Notificación no encontrada.");
            
            if (notification.UserId != userId)
                return ServiceResult.Failure("No tienes permiso para marcar esta notificación.");

            notification.IsRead = true;
            await _notificationRepo.UpdateAsync(notification);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> MarkAllAsReadAsync(string userId)
        {
            await _notificationRepo.MarkAllAsReadAsync(userId);
            return ServiceResult.Success();
        }
    }
}
