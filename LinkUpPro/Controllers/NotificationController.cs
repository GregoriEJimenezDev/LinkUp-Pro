using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkUpPro.Core.Application.Interfaces.IServices;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class NotificationController(
        INotificationService notificationService,
        IUserService userService) : BaseController
    {
        private readonly INotificationService _notificationService = notificationService;
        private readonly IUserService _userService = userService;

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
            await _notificationService.MarkAllAsReadAsync(userId);

            TempData["Success"] = "Todas las notificaciones han sido marcadas como leídas.";
            return RedirectToAction(nameof(Index));
        }
    }
}

