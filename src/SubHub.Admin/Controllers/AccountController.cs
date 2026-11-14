using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SubHub.Admin.Models;

namespace SubHub.Admin.Controllers;

public class AccountController : Controller
{
    private const string AuthenticationScheme = "SubHubAdminCookie";
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;

    public AccountController(IConfiguration configuration, IMemoryCache memoryCache)
    {
        _configuration = configuration;
        _memoryCache = memoryCache;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var clientKey = HttpContext.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "unknown";
        var attemptCacheKey = $"admin-login-failures:{clientKey}";
        if (_memoryCache.TryGetValue(attemptCacheKey, out int failedAttempts) && failedAttempts >= 5)
        {
            ModelState.AddModelError(string.Empty, "تعداد تلاش‌های ورود زیاد است؛ ۱۵ دقیقه دیگر دوباره تلاش کنید.");
            return View(model);
        }

        var configuredUsername = _configuration["AdminCredentials:Username"];
        var configuredPassword = _configuration["AdminCredentials:Password"];

        if (string.IsNullOrWhiteSpace(configuredUsername) || string.IsNullOrWhiteSpace(configuredPassword))
        {
            ModelState.AddModelError(string.Empty, "اطلاعات ورود مدیر تنظیم نشده است. متغیرهای AdminCredentials__Username و AdminCredentials__Password را پیکربندی کنید.");
            return View(model);
        }

        if (!FixedTimeEquals(model.Username, configuredUsername) ||
            !FixedTimeEquals(model.Password, configuredPassword))
        {
            _memoryCache.Set(attemptCacheKey, failedAttempts + 1, TimeSpan.FromMinutes(15));
            ModelState.AddModelError(string.Empty, "نام کاربری یا گذرواژه نادرست است.");
            return View(model);
        }

        _memoryCache.Remove(attemptCacheKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, configuredUsername),
            new Claim(ClaimTypes.Name, configuredUsername),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        await HttpContext.SignInAsync(
            AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [Authorize(AuthenticationSchemes = AuthenticationScheme)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private static bool FixedTimeEquals(string providedValue, string configuredValue)
    {
        var providedBytes = Encoding.UTF8.GetBytes(providedValue);
        var configuredBytes = Encoding.UTF8.GetBytes(configuredValue);
        return CryptographicOperations.FixedTimeEquals(providedBytes, configuredBytes);
    }
}
