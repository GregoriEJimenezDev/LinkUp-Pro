using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModel.Users
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Usuario requerido.")]
        public string Username { get; set; } = string.Empty;
    }
}
