using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModel.Users
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string LastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [RegularExpression(@"^(809|829|849)[\s-]?\d{3}[\s-]?\d{4}$", ErrorMessage = "Formato inválido. Usa (ejemplos): 809-000-0000 o 8290000000")]
        public string Phone { get; set; } = string.Empty;
        public byte[]? ProfilePicture { get; set; }
        public string? CurrentProfilePicture { get; set; }
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación de la contraseña no coinciden.")]
        public string? ConfirmPassword { get; set; }
    }
}
