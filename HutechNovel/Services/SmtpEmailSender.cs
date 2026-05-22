using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace HutechNovel.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpEmailOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpEmailOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            ValidateOptions();

            using var message = new MailMessage
            {
                From = new MailAddress(
                    string.IsNullOrWhiteSpace(_options.FromEmail) ? _options.UserName : _options.FromEmail,
                    _options.FromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            message.To.Add(email);

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.UserName, _options.Password)
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Sent email '{Subject}' to {Email}.", subject, email);
        }

        private void ValidateOptions()
        {
            if (string.IsNullOrWhiteSpace(_options.Host) ||
                string.IsNullOrWhiteSpace(_options.UserName) ||
                string.IsNullOrWhiteSpace(_options.Password))
            {
                throw new InvalidOperationException("SMTP email is not configured. Please set Smtp:Host, Smtp:UserName, and Smtp:Password.");
            }
        }
    }
}
