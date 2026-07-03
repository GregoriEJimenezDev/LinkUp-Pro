using LinkUpPro.Core.Application.ViewModel.Post;
using LinkUpPro.Core.Application.DTOs.Friend;
using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Application.ViewModel.Friend
{
    public class FriendIndexViewModel
    {
        public List<PostViewModel> FriendsPosts { get; set; } = [];
        public List<FriendDto> Friends { get; set; } = [];
        public string? SearchQuery { get; set; }
        public MediaType? FilterMediaType { get; set; }
        public DateTime? FilterDateFrom { get; set; }
        public DateTime? FilterDateTo { get; set; }
        public int? FilterEditState { get; set; }
        public string? FriendUsername { get; set; }
    }
}
