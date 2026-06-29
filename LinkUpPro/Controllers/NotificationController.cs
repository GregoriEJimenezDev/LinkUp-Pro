using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkUpPro.Core.Application.Interfaces.IServices;
using System.Security.Claims;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public NotificationController(
            INotificationService notificationService,
            IUserService userService)
        {
            _notificationService = notificationService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = UserId;
            var notifications = await _notificationService.GetNotificationsAsync(userId);
            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id, string? returnUrl)
        {
            var userId = UserId;
            await _notificationService.MarkAsReadAsync(id, userId);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = UserId;
            var notifications = await _notificationService.GetNotificationsAsync(userId);
            var unread = notifications.Where(n => !n.IsRead).ToList();

            foreach (var notification in unread)
            {
                await _notificationService.MarkAsReadAsync(notification.Id, userId);
            }

            TempData["Success"] = "Todas las notificaciones han sido marcadas como leídas.";
            return RedirectToAction(nameof(Index));
        }
    }
}

