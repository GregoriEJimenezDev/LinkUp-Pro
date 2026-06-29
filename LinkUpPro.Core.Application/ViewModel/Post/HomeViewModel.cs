using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Application.ViewModel.Post
{
    public class HomeViewModel
    {
        public List<PostViewModel> Posts { get; set; } = [];
        public string? SearchQuery { get; set; }
        public MediaType? FilterMediaType { get; set; }
        public DateTime? FilterDate { get; set; }
        public bool? FilterIsEdited { get; set; }
    }
}
