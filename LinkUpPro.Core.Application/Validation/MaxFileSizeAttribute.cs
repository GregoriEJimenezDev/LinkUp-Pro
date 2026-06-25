using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.Validation
{
    public class MaxFileSizeAttribute(int maxSizeInBytes) : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is byte[] fileBytes && fileBytes.Length > maxSizeInBytes)
            {
                var maxMB = maxSizeInBytes / (1024.0 * 1024.0);
                return new ValidationResult($"El archivo no debe superar {maxMB:F0} MB.");
            }
            return ValidationResult.Success;
        }
    }
}
