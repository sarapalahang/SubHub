using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileLoginOtp.Domain.Entities;
using MobileLoginOtp.Infrastructure.Data;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization; // Add this for [Authorize]

namespace MobileLoginOtp.Api.Controllers
{
    [Route("[controller]/[action]")]
    public class PaymentController : Controller
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(
            ILogger<PaymentController> logger,
            IHttpClientFactory httpClientFactory,
            AppDbContext context,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Recommended for security: ensures user is logged in
        public async Task<IActionResult> RequestPayment(long amount)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                // This part might not be reached if [Authorize] is used and user is not logged in
                // but good for safety if [Authorize] is removed or for other scenarios.
                return Content("کاربر وارد نشده است.");
            }

            _logger.LogInformation("شروع عملیات پرداخت برای مبلغ: {Amount}, UserId: {UserId}", amount, userId);

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return Content("کاربر پیدا نشد.");
            }

            var merchantId = _configuration["ZarinPal:MerchantId"];
            var callbackUrl = _configuration["ZarinPal:CallbackUrl"];

            if (string.IsNullOrWhiteSpace(merchantId) || string.IsNullOrWhiteSpace(callbackUrl))
            {
                return Content("تنظیمات درگاه پرداخت ناقص است.");
            }

            var amountInRial = amount * 10;

            var paymentData = new
            {
                merchant_id = merchantId,
                amount = amountInRial,
                callback_url = callbackUrl,
                description = "خرید تستی از سایت مترونوم"
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                var jsonPayload = JsonSerializer.Serialize(paymentData);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "https://api.zarinpal.com/pg/v4/payment/request.json",
                    content);

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("پاسخ زرین پال: {Response}", responseString);

                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                var data = root.GetProperty("data");
                var code = data.GetProperty("code").GetInt32();

                if (code != 100)
                {
                    string message = "خطا در ایجاد درخواست پرداخت.";

                    if (root.TryGetProperty("errors", out var errors) &&
                        errors.TryGetProperty("message", out var messageElement))
                    {
                        message = messageElement.GetString() ?? message;
                    }

                    return Content(message);
                }

                var authority = data.GetProperty("authority").GetString();
                if (string.IsNullOrWhiteSpace(authority))
                {
                    return Content("کد Authority دریافت نشد.");
                }

                var paymentLog = new PaymentLog
                {
                    UserId = user.Id,
                    Amount = amountInRial,
                    Authority = authority,
                    Description = "خرید تستی از سایت مترونوم",
                    IsPaid = false,
                    CreatedAt = DateTime.Now
                };

                _context.PaymentLogs.Add(paymentLog);
                await _context.SaveChangesAsync();

                return Redirect($"https://www.zarinpal.com/pg/StartPay/{authority}");
            }
            catch (Exception ex)
            {
                // این خط به شما می‌گوید دقیقاً چه چیزی نال است یا چه خطایی رخ داده
                return Content($"خطا: {ex.Message} -- Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerifyByHttpClient(string authority, string status)
        {
            _logger.LogInformation("شروع تایید تراکنش. Authority: {Authority}", authority);

            if (string.IsNullOrWhiteSpace(authority) || status != "OK")
            {
                return RedirectToAction("Account", "Auth", new { paid = 0 }); // به جای BadRequest کاربر را برگردانید
            }

            var paymentLog = await _context.PaymentLogs
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Authority == authority);

            if (paymentLog == null) return NotFound("تراکنش در سیستم یافت نشد.");
            if (paymentLog.IsPaid) return RedirectToAction("Account", "Auth", new { paid = 1 });

            var merchantId = _configuration["ZarinPal:MerchantId"];

            // مبلغ باید حتما به تومان باشد (اگر در دیتابیس ریال است، تقسیم بر 10 کنید)
            var verifyData = new
            {
                merchant_id = merchantId,
                amount = (int)paymentLog.Amount,
                authority = authority
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync("https://api.zarinpal.com/pg/v4/payment/verify.json", verifyData);
                var responseString = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                // بررسی وجود آبجکت data بدون کرش کردن
                if (root.TryGetProperty("data", out var data) && data.ValueKind != JsonValueKind.Null)
                {
                    if (data.TryGetProperty("code", out var codeElement))
                    {
                        int code = codeElement.GetInt32();
                        if (code == 100 || code == 101)
                        {
                            string refId = data.GetProperty("ref_id").ToString();

                            // آپدیت دیتابیس
                            paymentLog.IsPaid = true;
                            paymentLog.RefId = refId;
                            paymentLog.PaidAt = DateTime.Now;

                            if (paymentLog.User != null)
                            {
                                paymentLog.User.IsActive = true;
                                paymentLog.User.ActivatedAt = DateTime.Now;
                            }

                            await _context.SaveChangesAsync();

                            // به جای بازگشت Content، کاربر را به پروفایل بفرستید
                            return RedirectToAction("Account", "Auth", new { paid = 1, refid = refId });
                        }
                    }
                }

                // اگر به اینجا برسد یعنی پرداخت تایید نشده
                return RedirectToAction("Account", "Auth", new { paid = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Crash in VerifyByHttpClient");
                return Content("خطای داخلی سرور رخ داد. لطفا با پشتیبانی تماس بگیرید.");
            }
        }

    }
}
