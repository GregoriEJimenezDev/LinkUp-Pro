using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModel.Users
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasea es obligatoria.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "La contrasea debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "La contrasea debe tener al menos una mayscula, una minscula, un nmero y un carcter especial.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmacin de la contrasea tambin es obligatoria.")]
        [MinLength(8, ErrorMessage = "La confirmacin de la contrasea debe tener al menos 8 caracteres.")]
        [Compare("Password", ErrorMessage = "Las contraseas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
