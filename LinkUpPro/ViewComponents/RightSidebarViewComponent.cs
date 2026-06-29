using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.ViewModel.Friend;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.ViewComponents
{
    public class RightSidebarViewComponent : ViewComponent
    {
        private readonly IBattleshipService _battleshipService;
        private readonly IFriendRequestService _friendRequestService;

        public RightSidebarViewComponent(
            IBattleshipService battleshipService,
            IFriendRequestService friendRequestService)
        {
            _battleshipService = battleshipService;
            _friendRequestService = friendRequestService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = Request.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var vm = new RightSidebarViewModel();

            if (userId != null)
            {
                var gameIndex = await _battleshipService.GetIndexAsync(userId);
                vm.ActiveGames = gameIndex?.ActiveGames?.Take(3).ToList() ?? new List<GameDto>();

                var suggestions = await _friendRequestService.GetAvailableUsersAsync(userId, null);
                vm.SuggestedUsers = suggestions.AvailableUsers.Take(3).ToList();
            }

            return View(vm);
        }
    }

    public class RightSidebarViewModel
    {
        public List<GameDto> ActiveGames { get; set; } = new();
        public List<UserToAddViewModel> SuggestedUsers { get; set; } = new();
    }
}
