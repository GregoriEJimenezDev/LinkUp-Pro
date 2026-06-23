using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Interfaces.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailRequest request);
    }
}
