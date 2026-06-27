using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LinkUpPro.Infrastructure.Shared.EmailServices
{
    public class EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger) : IEmailService
    {
        private readonly EmailSettings _settings = settings.Value;
        private readonly ILogger<EmailService> _logger = logger;

        public async Task SendEmailAsync(EmailRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.SmtpHost))
                {
                    _logger.LogWarning("SmtpHost is empty. Skipping email sending.");
                    return;
                }

                var email = new MimeMessage();

                email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                email.To.Add(MailboxAddress.Parse(request.To));
                email.Subject = request.Subject;

                var builder = new BodyBuilder();

                if (request.IsHtml)
                    builder.HtmlBody = request.Body;
                else
                    builder.TextBody = request.Body;

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls );

                await smtp.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", request.To);
                throw new Exception("Ha ocurrido un error al enviar el correo electrónico.");
            }
        }
    }
}
