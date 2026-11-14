using Microsoft.Extensions.Configuration;
using SubHub.Application.Interfaces;
using SubHub.Application.Models;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SubHub.Infrastructure.Services;

public class ZarinPalPaymentGatewayService : IPaymentGatewayService
{
    public string ProviderName => "ZarinPal";

    private readonly HttpClient _httpClient;
    private readonly string _merchantId;

    public ZarinPalPaymentGatewayService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _merchantId = configuration["ZarinPal:MerchantId"] ?? string.Empty;
    }

    public async Task<(bool IsSuccess, string? Authority, string? PaymentUrl, string? ErrorMessage)> RequestPaymentAsync(
        long amount, string description, string callbackUrl, string? userEmail = null, string? userMobile = null)
    {
        try
        {
            if (amount <= 0)
                return (false, null, null, "مبلغ باید بزرگتر از صفر باشد.");
            if (string.IsNullOrWhiteSpace(_merchantId))
                return (false, null, null, "تنظیمات درگاه پرداخت کامل نیست.");

            var requestParameters = new RequestParameters(
                _merchantId,
                amount * 10, // تبدیل تومان به ریال
                description,
                callbackUrl,
                userMobile,
                userEmail);

            var response = await _httpClient.PostAsJsonAsync(URLs.requestUrl, requestParameters);
            var responseText = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ZarinPalRequestResponse>(responseText);

            if (result?.Data?.Code == 100 && !string.IsNullOrWhiteSpace(result.Data.Authority))
            {
                return (true, result.Data.Authority, $"{URLs.gateWayUrl}{result.Data.Authority}", null);
            }

            return (false, null, null, $"خطا در زرین‌پال: {responseText}");
        }
        catch (Exception ex)
        {
            return (false, null, null, ex.Message);
        }
    }

    public async Task<(bool IsSuccess, string? RefId, string? ErrorMessage)> VerifyPaymentAsync(string authority, long amount)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_merchantId))
                return (false, null, "تنظیمات درگاه پرداخت کامل نیست.");

            var verifyParameters = new VerifyParameters
            {
                merchant_id = _merchantId,
                amount = (amount * 10).ToString(), // تبدیل تومان به ریال به صورت رشته
                authority = authority
            };

            var response = await _httpClient.PostAsJsonAsync(URLs.verifyUrl, verifyParameters);
            var responseText = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ZarinPalVerifyResponse>(responseText);

            // کد 100: اولین تأیید موفق | کد 101: قبلاً با موفقیت تأیید شده است
            if (result?.Data?.Code == 100 || result?.Data?.Code == 101)
            {
                return (true, result.Data.RefId.ToString(), null);
            }

            return (false, null, $"خطا در تایید: {responseText}");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// استعلام وضعیت تراکنش‌های معلق از زرین‌پال
    /// </summary>
    public async Task<(bool IsPaid, string? RefId, string? ErrorMessage)> InquiryPaymentAsync(string authority)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(authority))
                return (false, null, "شناسه اتوریتی نامعتبر است.");
            if (string.IsNullOrWhiteSpace(_merchantId))
                return (false, null, "تنظیمات درگاه پرداخت کامل نیست.");

            var inquiryParameters = new
            {
                merchant_id = _merchantId,
                authority = authority
            };

            var response = await _httpClient.PostAsJsonAsync(URLs.inquiryUrl, inquiryParameters);
            var responseText = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ZarinPalInquiryResponse>(responseText);

            if (result?.Data is null)
            {
                return (false, null, $"خطا در دریافت وضعیت از زرین‌پال: {responseText}");
            }

            // طبق مستندات زرین‌پال:
            // اگر VERIFIED باشد یعنی قبلاً وریفای شده است
            // اگر PAID باشد یعنی در بانک پرداخت شده اما هنوز وریفای نشده است
            string status = result.Data.Status?.ToUpperInvariant() ?? string.Empty;

            if (status == "VERIFIED" || status == "PAID")
            {
                // در صورت وجود RefId در دیتا از آن استفاده می‌کنیم، در غیر این‌صورت شناسه موقت ثبت می‌شود
                string refId = result.Data.RefId > 0 ? result.Data.RefId.ToString() : authority;
                return (true, refId, null);
            }

            return (false, null, $"وضعیت تراکنش: {status}");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    // مدل‌های خصوصی نگاشت پاسخ‌های زرین‌پال
    private sealed class ZarinPalRequestResponse
    {
        [JsonPropertyName("data")] public RequestData? Data { get; set; }
    }

    private sealed class RequestData
    {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("authority")] public string? Authority { get; set; }
    }

    private sealed class ZarinPalVerifyResponse
    {
        [JsonPropertyName("data")] public VerifyData? Data { get; set; }
    }

    private sealed class VerifyData
    {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("ref_id")] public long RefId { get; set; }
    }

    private sealed class ZarinPalInquiryResponse
    {
        [JsonPropertyName("data")] public InquiryData? Data { get; set; }
    }

    private sealed class InquiryData
    {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("ref_id")] public long RefId { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
    }
}
