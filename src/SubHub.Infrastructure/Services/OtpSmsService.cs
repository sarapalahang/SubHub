using Kavenegar;
using Microsoft.Extensions.Configuration;
using SubHub.Application.Interfaces;

namespace SubHub.Infrastructure.Services;

public class OtpSmsService : ISmsService
{
    private readonly IConfiguration _configuration;

    public OtpSmsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendInvoiceLinkSmsAsync(string userPhoneNumber, string invoiceNumber, string invoiceUrl)
    {
        await SendSmsAsync(userPhoneNumber, $"فاکتور {invoiceNumber} - لینک: {invoiceUrl}");
    }

    public async Task SendSmsAsync(string userPhoneNumber, string message)
    {
        var apiKey = _configuration["Kavenegar:ApiKey"];
        var template = _configuration["Kavenegar:Template"];
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(template))
            throw new InvalidOperationException("Kavenegar SMS settings are not configured.");

        var kavenegar = new KavenegarApi(apiKey);
        await kavenegar.VerifyLookup(userPhoneNumber, message, template);
    }
}
