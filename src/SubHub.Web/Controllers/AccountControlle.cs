using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SubHub.Application.Interfaces;
using SubHub.Domain.Entities;

namespace SubHub.Web.Controllers;

public class AccountController : Controller
{
    private const string CookieScheme = "SubHubCookie";
    private const int MaximumOtpAttempts = 5;
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan OtpSendInterval = TimeSpan.FromSeconds(60);

    private readonly IApplicationDbContext _context;
    private readonly ISmsService _smsService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AccountController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        IApplicationDbContext context,
        ISmsService smsService,
        IMemoryCache memoryCache,
        ILogger<AccountController> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _smsService = smsService;
        _memoryCache = memoryCache;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string phoneNumber, string? returnUrl = null)
    {
        phoneNumber = phoneNumber?.Trim() ?? string.Empty;
        ViewData["ReturnUrl"] = returnUrl;

        if (phoneNumber.Length > 16 || !Regex.IsMatch(phoneNumber, @"^\+?[0-9]{10,15}$"))
        {
            ModelState.AddModelError(string.Empty, "شماره موبایل معتبر وارد کنید.");
            return View();
        }

        var phoneHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(phoneNumber)));
        var sendLimitKey = $"otp-send:{phoneHash}";
        if (!_environment.IsDevelopment() && _memoryCache.TryGetValue(sendLimitKey, out _))
        {
            ModelState.AddModelError(string.Empty, "برای ارسال دوبارهٔ کد یک دقیقه صبر کنید.");
            return View();
        }

        var isDevelopment = _environment.IsDevelopment();
        var code = isDevelopment
            ? "12345"
            : RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var challengeId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var challenge = new OtpChallenge(
            phoneNumber,
            SHA256.HashData(Encoding.UTF8.GetBytes(code)));

        _memoryCache.Set(GetChallengeKey(challengeId), challenge, OtpLifetime);
        if (!_environment.IsDevelopment())
            _memoryCache.Set(sendLimitKey, true, OtpSendInterval);

        try
        {
            if (!isDevelopment)
                await _smsService.SendSmsAsync(phoneNumber, code);
        }
catch (Exception exception)
        {
            _memoryCache.Remove(GetChallengeKey(challengeId));
    _logger.LogError(
        "OTP SMS request failed. Error type: {ErrorType}; inner error type: {InnerErrorType}",
        exception.GetType().FullName,
        exception.InnerException?.GetType().FullName ?? "(none)");
            ModelState.AddModelError(string.Empty, "ارسال کد تایید انجام نشد. تنظیمات پیامک را بررسی کنید.");
            return View();
        }

        TempData["OtpChallengeId"] = challengeId;
        TempData["ReturnUrl"] = returnUrl;
        return RedirectToAction(nameof(VerifyOtp));
    }

    [HttpGet]
    public IActionResult VerifyOtp()
    {
        var challengeId = TempData["OtpChallengeId"]?.ToString();
        if (string.IsNullOrWhiteSpace(challengeId) ||
            !_memoryCache.TryGetValue(GetChallengeKey(challengeId), out OtpChallenge? challenge) ||
            challenge is null)
        {
            TempData["AuthError"] = "کد تایید منقضی شده است؛ دوباره درخواست کد کنید.";
            return RedirectToAction(nameof(Login));
        }

        TempData.Keep("OtpChallengeId");
        TempData.Keep("ReturnUrl");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(string code)
    {
        var challengeId = TempData["OtpChallengeId"]?.ToString();
        var returnUrl = TempData["ReturnUrl"]?.ToString();
        if (string.IsNullOrWhiteSpace(challengeId) ||
            !_memoryCache.TryGetValue(GetChallengeKey(challengeId), out OtpChallenge? challenge) ||
            challenge is null)
        {
            TempData["AuthError"] = "کد تایید منقضی شده است؛ دوباره درخواست کد کنید.";
            return RedirectToAction(nameof(Login));
        }

        var submittedCode = code ?? string.Empty;
        var submittedHash = SHA256.HashData(Encoding.UTF8.GetBytes(submittedCode));
        var expectedCodeLength = _environment.IsDevelopment() ? 5 : 6;
        var isValidCode = submittedCode.Length == expectedCodeLength &&
            submittedCode.All(char.IsAsciiDigit) &&
            CryptographicOperations.FixedTimeEquals(submittedHash, challenge.CodeHash);

        if (!isValidCode)
        {
            challenge.FailedAttempts++;
            if (challenge.FailedAttempts >= MaximumOtpAttempts)
            {
                _memoryCache.Remove(GetChallengeKey(challengeId));
                TempData["AuthError"] = "تعداد تلاش‌ها بیش از حد مجاز بود؛ کد تازه درخواست کنید.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(string.Empty, "کد تایید واردشده معتبر نیست.");
            TempData.Keep("OtpChallengeId");
            TempData.Keep("ReturnUrl");
            return View();
        }

        _memoryCache.Remove(GetChallengeKey(challengeId));

        var user = await _context.Users.FirstOrDefaultAsync(user => user.PhoneNumber == challenge.PhoneNumber);
        if (user is null)
        {
            user = new User
            {
                Name = "کاربر جدید",
                PhoneNumber = challenge.PhoneNumber
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name ?? "کاربر"),
            new(ClaimTypes.MobilePhone, user.PhoneNumber)
        };

        await HttpContext.SignInAsync(
            CookieScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieScheme)),
            new AuthenticationProperties { IsPersistent = false });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieScheme);
        return RedirectToAction(nameof(Login));
    }

    private static string GetChallengeKey(string challengeId) => $"otp-challenge:{challengeId}";

    private sealed class OtpChallenge(string phoneNumber, byte[] codeHash)
    {
        public string PhoneNumber { get; } = phoneNumber;
        public byte[] CodeHash { get; } = codeHash;
        public int FailedAttempts { get; set; }
    }
}
