using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModel.Users
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Usuario es requerido.")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Contraseña es requerida.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}