using System.ComponentModel.DataAnnotations;
using LinkUpPro.Core.Application.Validation;

namespace LinkUpPro.Core.Application.ViewModel.Users
{
    public class EditProfileViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Debe ingresar su nombre.")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Debe ingresar su apellido.")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Debe ingresar un número telefónico válido de República Dominicana.")]
        [RegularExpression(@"^(809|829|849)[\s-]?\d{3}[\s-]?\d{4}$", ErrorMessage = "Debe ingresar un número telefónico válido de República Dominicana.")]
        public string Phone { get; set; } = string.Empty;
        
        public byte[]? ProfilePicture { get; set; }
        public string? ProfilePictureFileName { get; set; }
        public string? CurrentProfilePicture { get; set; }
        
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }
        
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string? ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                yield return new ValidationResult("Debe ingresar su nombre.", new[] { nameof(FirstName) });
            }
            
            if (string.IsNullOrWhiteSpace(LastName))
            {
                yield return new ValidationResult("Debe ingresar su apellido.", new[] { nameof(LastName) });
            }

            bool isAnyPasswordFieldFilled = !string.IsNullOrEmpty(CurrentPassword) || 
                                            !string.IsNullOrEmpty(Password) || 
                                            !string.IsNullOrEmpty(ConfirmPassword);

            if (isAnyPasswordFieldFilled)
            {
                if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
                {
                    yield return new ValidationResult("Para cambiar su contraseña debe completar la contraseña actual, la nueva contraseña y su confirmación.", new[] { nameof(CurrentPassword), nameof(Password), nameof(ConfirmPassword) });
                }
                else if (CurrentPassword == Password)
                {
                    yield return new ValidationResult("La nueva contraseña no puede ser igual a la actual.", new[] { nameof(Password) });
                }
            }
        }
    }
}

