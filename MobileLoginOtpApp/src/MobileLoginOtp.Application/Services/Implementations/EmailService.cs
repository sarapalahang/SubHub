using Microsoft.Extensions.Options;
using MobileLoginOtp.Domain.Interfaces;
using System.Net;
using System.Net.Mail;

namespace MobileLoginOtp.Application.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = _smtpSettings.EnableSsl,
                Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 20000
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.From),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}



