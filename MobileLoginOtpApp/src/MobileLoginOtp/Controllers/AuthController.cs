using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileLoginOtp.Application.Dtos;
using MobileLoginOtp.Application.Dtos.ViewModels;
using MobileLoginOtp.Application.Interfaces;
using MobileLoginOtp.Domain.Entities;
using MobileLoginOtp.Domain.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;

namespace MobileLoginOtp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService; // تنها واسط ما با داده‌ها
        private readonly ISmsService _smsService;

        // ریپازیتوری کاملاً از اینجا حذف شد
        public AuthController(IOtpService otpService, IUserService userService, IEmailService emailService, ISmsService smsService)
        {
            _otpService = otpService;
            _emailService = emailService;
            _smsService = smsService;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpGet]
        public IActionResult Verify(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return RedirectToAction("Index", "Role");

            ViewBag.PhoneNumber = phoneNumber;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmailOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { success = false, message = "ایمیل را وارد کنید." });

            email = email.Trim().ToLower();

            // به جای کار با ریپازیتوری، از سرویس می‌خواهیم کاربر را بدهد یا بسازد
            var user = await _userService.GetOrCreateByEmailAsync(email);

            var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.EmailOtpCode = otpCode;
            user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(5);

            await _userService.SaveChangesAsync(); // استفاده از سرویس برای ذخیره

            try
            {
                await _emailService.SendEmailAsync(email, "کد تایید", $"کد: <b>{otpCode}</b>");
                return Json(new { success = true, message = "کد تایید ارسال شد." });
            }
            catch
            {
                return Json(new { success = false, message = "خطا در ارسال ایمیل." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmailOtp(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                return Json(new OtpResultDto { IsSuccess = false, Message = "اطلاعات ناقص است." });

            var user = await _userService.GetByEmailAsync(email.Trim().ToLower());

            if (user == null || user.EmailOtpCode != code || user.EmailOtpExpiry < DateTime.UtcNow)
                return Json(new OtpResultDto { IsSuccess = false, Message = "کد نامعتبر یا منقضی شده است." });

            user.EmailConfirmed = true;
            user.EmailOtpCode = null;
            user.LastLoginAtUtc = DateTime.UtcNow;

            await _userService.UpdateAsync(user); // سپردن آپدیت به سرویس

            await SignInUserAsync(user);

            var isAdmin = await _userService.IsAdminAsync(user.Id);
            return Json(new OtpResultDto
            {
                IsSuccess = true,
                RedirectUrl = isAdmin ? Url.Action("Index", "Role") : Url.Action("Account", "Auth")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendSmsOtp(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return Json(new OtpResultDto { IsSuccess = false, Message = "شماره را وارد کنید." });

            var user = await _userService.GetOrCreateByPhoneNumberAsync(phoneNumber.Trim());

            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            user.SmsOtpCode = otpCode;
            user.SmsOtpExpiry = DateTime.UtcNow.AddMinutes(5);

            await _userService.SaveChangesAsync();

            var isSent = await _smsService.SendOtpSmsAsync(user.PhoneNumber, otpCode);
            return Json(new OtpResultDto { IsSuccess = isSent, Message = isSent ? "کد پیامک شد." : "خطا در ارسال." });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await _userService.GetByEmailAsync(model.Email);
            if (user != null)
            {
                user.ResetPasswordToken = Guid.NewGuid().ToString();
                user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(30);
                await _userService.UpdateAsync(user);

                var resetLink = Url.Action("ResetPassword", "Auth", new { token = user.ResetPasswordToken }, Request.Scheme);
                await _emailService.SendEmailAsync(user.Email, "بازیابی رمز", $"لینک: {resetLink}");
            }
            return View("ForgotPasswordConfirmation");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out var userId)) return RedirectToAction("Login");

            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return RedirectToAction("Login");

            return View(new ProfileViewModel { /* مپ کردن مقادیر مثل قبل */ });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfile(ProfileViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // چک کردن نال نبودن userIdStr قبل از Parse کردن
            if (string.IsNullOrEmpty(userIdStr)) return NotFound();

            var user = await _userService.GetByIdAsync(long.Parse(userIdStr));
            if (user == null) return NotFound();

            // ادامه منطق ذخیره پروفایل...
            return RedirectToAction("Account");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PaymentRequest(long amount)
    => Json(new { ok = true, amount });

        // سایر متدهای کمکی مثل SignInUserAsync هم باید از _userService.IsAdminAsync استفاده کنند
        private async Task SignInUserAsync(User user)
        {
            // تعریف لیست claims قبل از استفاده (رفع خطای CS0103)
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Email ?? user.PhoneNumber ?? "User")
    };

            var isAdmin = await _userService.IsAdminAsync(user.Id);
            if (isAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

    }
}
