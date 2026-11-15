using System.Diagnostics;
using Microsoft.Extensions.Options;
using MobileLoginOtp.Domain.Interfaces;



namespace MobileLoginOtp.Application.Services.Implementations
{
    public class SmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public SmsService(HttpClient httpClient, IOptions<SmsSettings> settings)
        {
            _httpClient = httpClient;
            _apiKey = settings.Value.ApiKey;
            // تنظیم حداکثر زمان انتظار روی ۶ ثانیه جهت جلوگیری از کندی و معطلی کاربر
            _httpClient.Timeout = TimeSpan.FromSeconds(6);
        }

        public async Task<bool> SendOtpSmsAsync(string mobile, string code)
        {
            try
            {
                // ۱. آماده‌سازی آدرس وب‌سرویس کاوه‌نگار
                var url = $"https://api.kavenegar.com/v1/{_apiKey}/verify/lookup.json?receptor={mobile}&token={code}&template=verify";

                // ۲. ارسال درخواست با رعایت زمان انتظار
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[Kavenegar] SMS Sent Successfully to {mobile}. Code: {code}");
                    return true;
                }
                else
                {
                    // اگر کاوه‌نگار خطایی برگرداند، متن خطا را بخوانیم تا بفهمیم مشکل از کجاست
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[Kavenegar Error] Status: {response.StatusCode}, Content: {errorContent}");
                    Console.WriteLine($"[Kavenegar Error] Status: {response.StatusCode}, Content: {errorContent}");

                    // راهکار کمکی در زمان توسعه (اگر پنل پیامک خطا داد، برای تست کد را اینجا ببینید)
                    ShowMockCodeInConsole(mobile, code);
                    return false;
                }
            }
            catch (TaskCanceledException)
            {
                // زمان درخواست به پایان رسیده (Timeout رخ داده است)
                Debug.WriteLine("[Kavenegar Error] Request Timeout! Kavenegar API did not respond within 6 seconds.");
                ShowMockCodeInConsole(mobile, code);
                return false;
            }
            catch (Exception ex)
            {
                // هر خطای دیگری مثل قطع بودن اینترنت یا SSL
                Debug.WriteLine($"[Kavenegar Exception] Message: {ex.Message}");
                ShowMockCodeInConsole(mobile, code);
                return false;
            }
        }

        // متد کمکی برای تست راحت‌تر شما (در پنجره Output ویژوال استودیو کد تایید را چاپ می‌کند)
        private void ShowMockCodeInConsole(string mobile, string code)
        {
            Debug.WriteLine("\n==================================================");
            Debug.WriteLine($"[TEST MODE] SMS failed to send via Kavenegar.");
            Debug.WriteLine($"Use this OTP code to login for ({mobile}): {code}");
            Debug.WriteLine("==================================================\n");
        }
    }
}



