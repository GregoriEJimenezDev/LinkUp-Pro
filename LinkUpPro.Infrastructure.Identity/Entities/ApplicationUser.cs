using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Infrastructure.Identity.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public bool IsActive { get; set; } = false;
        public string? ActivationToken { get; set; }
        public string? ResetPasswordToken { get; set; }

    }
}
