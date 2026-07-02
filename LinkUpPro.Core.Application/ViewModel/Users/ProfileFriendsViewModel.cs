using System.Collections.Generic;
using LinkUpPro.Core.Application.DTOs.Friend;
using LinkUpPro.Core.Application.DTOs.User;

namespace LinkUpPro.Core.Application.ViewModel.Users
{
    public class ProfileFriendsViewModel
    {
        public UserBasicDto UserInfo { get; set; } = null!;
        public List<FriendDto> Friends { get; set; } = [];
        public bool IsCurrentUser { get; set; }
    }
}
