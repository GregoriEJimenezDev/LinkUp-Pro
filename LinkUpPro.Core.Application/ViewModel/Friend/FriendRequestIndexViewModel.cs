using LinkUpPro.Core.Application.DTOs.Friend;

namespace LinkUpPro.Core.Application.ViewModel.Friend
{
    public class FriendRequestIndexViewModel
    {
        public List<FriendRequestDto> Received { get; set; } = [];
        public List<FriendRequestDto> Sent { get; set; } = [];
    }
}
