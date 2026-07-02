using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUpPro.ViewComponents
{
    public class AppSidebarViewComponent(
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService,
        IFriendRequestService friendRequestService) : ViewComponent
    {
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IFriendRequestService _friendRequestService;

        public AppSidebarViewComponent(
            IUserService userService,
            INotificationService notificationService,
            IFriendRequestService friendRequestService)
        {
            _userService = userService;
            _notificationService = notificationService;
            _friendRequestService = friendRequestService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = Request.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            UserBasicDto? currentUser = null;
            int unreadNotificationsCount = 0;
            int pendingRequestsCount = 0;

            if (userId != null)
            {
                currentUser = await _userService.GetUserBasicInfoAsync(userId);
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
        public UserBasicDto? CurrentUser { get; set; }
        public int UnreadNotificationsCount { get; set; }
        public int PendingFriendRequestsCount { get; set; }
    }
}
