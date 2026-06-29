using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModel.Friend
{
    public class SendFriendRequestViewModel
    {
        [Required(ErrorMessage = "Por favor, selecciona un usuario para enviarle la solicitud de amistad.")]
        public string SelectedUserId { get; set; } = string.Empty;
        public string? SearchUsername { get; set; }
        public List<UserToAddViewModel> AvailableUsers { get; set; } = [];
    }

    public class UserToAddViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public int MutualFriendsCount { get; set; }
    }
}
