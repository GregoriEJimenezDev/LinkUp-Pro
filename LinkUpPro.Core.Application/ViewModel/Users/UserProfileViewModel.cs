using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.ViewModel.Post;

namespace LinkUpPro.Core.Application.ViewModel.Users
{
    public class UserProfileViewModel
    {
        public UserBasicDto UserInfo { get; set; } = new();
        public int FriendsCount { get; set; }
        public int PostsCount { get; set; }
        public List<PostViewModel> Posts { get; set; } = new();
        
        // Friendship status between the logged-in user and the profile owner
        public bool IsFriend { get; set; }
        public bool IsPendingRequestSent { get; set; }
        public bool IsPendingRequestReceived { get; set; }
    }
}
