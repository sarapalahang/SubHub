using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SubHub.Application.Interfaces;

namespace SubHub.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendInvoiceEmailAsync(string toEmail, string invoiceNumber, string invoiceUrl)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return;

        var settings = GetSmtpSettings();
        using var client = CreateClient(settings);
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(settings.From, "SubHub"),
            To = { toEmail },
            Subject = $"فاکتور خرید اشتراک - شماره {invoiceNumber}",
            Body = $"""
                <div dir="rtl" style="font-family: Tahoma, sans-serif; padding: 20px; line-height: 2;">
                    <h3>با تشکر از خرید شما</h3>
                    <p>فاکتور شما با شماره <b>{WebUtility.HtmlEncode(invoiceNumber)}</b> با موفقیت صادر شد.</p>
                    <p><a href="{WebUtility.HtmlEncode(invoiceUrl)}">مشاهده فاکتور</a></p>
                </div>
                """,
            IsBodyHtml = true
        };

        await client.SendMailAsync(mailMessage);
    }

    public async Task SendInvoiceEmailWithPdfAsync(string toEmail, string invoiceNumber, byte[]? pdfBytes)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return;

        var settings = GetSmtpSettings();
        using var client = CreateClient(settings);
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(settings.From, "SubHub"),
            To = { toEmail },
            Subject = $"رسید پرداخت و فاکتور اشتراک - شماره {invoiceNumber}",
            Body = "<div dir=\"rtl\"><h3>با تشکر از خرید شما</h3><p>فاکتور خرید به این ایمیل پیوست شده است.</p></div>",
            IsBodyHtml = true
        };

        if (pdfBytes is { Length: > 0 })
        {
            var memoryStream = new MemoryStream(pdfBytes);
            mailMessage.Attachments.Add(new Attachment(
                memoryStream,
                $"Invoice-{WebUtility.HtmlEncode(invoiceNumber)}.pdf",
                "application/pdf"));
        }

        await client.SendMailAsync(mailMessage);
    }

    public async Task SendSubscriptionExpiryEmailAsync(string toEmail, string planName, int daysRemaining)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return;

        var settings = GetSmtpSettings();
        using var client = CreateClient(settings);
        var safePlanName = WebUtility.HtmlEncode(planName);
        var dayText = daysRemaining == 1 ? "فردا" : $"تا {daysRemaining} روز دیگر";
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(settings.From, "SubHub"),
            To = { toEmail },
            Subject = "یادآوری پایان اشتراک",
            Body = $"""
                <div dir="rtl" style="font-family: Tahoma, sans-serif; padding: 20px; line-height: 2;">
                    <h3>یادآوری پایان اشتراک</h3>
                    <p>اشتراک طرح <b>{safePlanName}</b> {dayText} به پایان می‌رسد.</p>
                    <p>برای ادامه استفاده از سرویس، اشتراک خود را تمدید کنید.</p>
                </div>
                """,
            IsBodyHtml = true
        };

        await client.SendMailAsync(mailMessage);
    }

    private SmtpSettings GetSmtpSettings()
    {
        var host = _configuration["SmtpSettings:Host"];
        var portValue = _configuration["SmtpSettings:Port"];
        var userName = _configuration["SmtpSettings:UserName"];
        var password = _configuration["SmtpSettings:Password"];
        var from = _configuration["SmtpSettings:From"];
        var enableSslValue = _configuration["SmtpSettings:EnableSsl"];

        if (string.IsNullOrWhiteSpace(host) ||
            !int.TryParse(portValue, out var port) ||
            string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(password) ||
            !bool.TryParse(enableSslValue, out var enableSsl) ||
            !enableSsl)
        {
            throw new InvalidOperationException("Secure SMTP settings are not configured.");
        }

        return new SmtpSettings(host, port, userName, password, string.IsNullOrWhiteSpace(from) ? userName : from, enableSsl);
    }

    private static SmtpClient CreateClient(SmtpSettings settings)
    {
        return new SmtpClient(settings.Host, settings.Port)
        {
            Credentials = new NetworkCredential(settings.UserName, settings.Password),
            EnableSsl = settings.EnableSsl
        };
    }

    private sealed record SmtpSettings(
        string Host,
        int Port,
        string UserName,
        string Password,
        string From,
        bool EnableSsl);
}
