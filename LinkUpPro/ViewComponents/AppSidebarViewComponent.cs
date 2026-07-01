using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.ViewComponents
{
    public class AppSidebarViewComponent(
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService,
        IFriendRequestService friendRequestService) : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly INotificationService _notificationService = notificationService;
        private readonly IFriendRequestService _friendRequestService = friendRequestService;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = await _userManager.GetUserAsync(Request.HttpContext.User);
            var userId = currentUser?.Id;

            int unreadNotificationsCount = 0;
            int pendingRequestsCount = 0;

            if (userId != null)
            {
                unreadNotificationsCount = await _notificationService.GetUnreadCountAsync(userId);
                pendingRequestsCount = await _friendRequestService.GetPendingCountAsync(userId);
            }

            var vm = new AppSidebarViewModel
            {
                CurrentUser = currentUser,
                UnreadNotificationsCount = unreadNotificationsCount,
                PendingFriendRequestsCount = pendingRequestsCount
            };

            return View(vm);
        }
    }

    public class AppSidebarViewModel
    {
        public ApplicationUser? CurrentUser { get; set; }
        public int UnreadNotificationsCount { get; set; }
        public int PendingFriendRequestsCount { get; set; }
    }
}
