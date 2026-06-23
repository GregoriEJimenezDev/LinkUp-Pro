using LinkUpPro.Core.Application.ViewModel.Post;
using LinkUpPro.Core.Application.DTOs.Friend;

namespace LinkUpPro.Core.Application.ViewModel.Friend
{
    public class FriendIndexViewModel
    {
        public List<PostViewModel> FriendsPosts { get; set; } = [];
        public List<FriendDto> Friends { get; set; } = [];
    }
}