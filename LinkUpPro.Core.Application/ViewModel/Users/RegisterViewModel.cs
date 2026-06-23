using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModel.Users
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [Display(Name = "Apellido")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La confirmación de la contraseña debe tener al menos 8 caracteres.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [RegularExpression(@"^(809|829|849)[\s-]?\d{3}[\s-]?\d{4}$", ErrorMessage = "Formato inválido. Usa ejemplos como: 809-000-0000 o 8290000000")]
        [Display(Name = "Número de teléfono")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Foto de perfil")]
        public byte[]? ProfilePicture { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [Display(Name = "Nombre de usuario")]
        public string Username { get; set; } = string.Empty;
    }
}
