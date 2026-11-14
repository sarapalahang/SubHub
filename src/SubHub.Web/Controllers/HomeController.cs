using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubHub.Application.Interfaces;
using SubHub.Domain.Enums;
using System.Diagnostics;

namespace SubHub.Web.Controllers
{
    // [Authorize] // اگر سیستم احراز هویت دارید این را فعال کنید تا کاربر لاگین‌کرده شناسایی شود
    public class HomeController : Controller
    {
        private readonly IApplicationDbContext _context;

        public HomeController(IApplicationDbContext context)
        {
            _context = context;
        }

        //public async Task<IActionResult> Index(CancellationToken ct)
        //{
        //    // ۱. دریافت شناسه کاربر جاری به صورت string و تبدیل آن به Guid (چون UserId در فاکتورها Guid است)
        //    string rawUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString();

        //    // اگر سیستم شما از Guid برای UserId استفاده می‌کند:
        //    Guid currentUserId = Guid.TryParse(rawUserId, out var parsedGuid) ? parsedGuid : Guid.Empty;

        //    var now = DateTime.UtcNow;

        //    // ۲. اشتراک جاری کاربر
        //    var activeSubscription = await _context.Subscriptions
        //        .Include(s => s.Plan)
        //        .Where(s => s.UserId.ToString() == rawUserId && s.Status == SubscriptionStatus.Active && s.EndDateUtc > now)
        //        .OrderByDescending(s => s.EndDateUtc)
        //        .FirstOrDefaultAsync(ct);

        //    // ۳. تاریخچه خرید (فاکتورهای کاربر)
        //    var invoices = await _context.Invoices
        //        .Include(i => i.Plan)
        //        .Where(i => i.UserId == currentUserId)
        //        .OrderByDescending(i => i.CreatedAtUtc)
        //        .Take(5)
        //        .ToListAsync(ct);

        //    ViewBag.ActiveSubscription = activeSubscription;
        //    ViewBag.Invoices = invoices;

        //    return View();
        //}
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            string rawUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString();
            Guid currentUserId = Guid.TryParse(rawUserId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            var now = DateTime.UtcNow;

            // بررسی موقت برای دیباگ: گرفتن همه اشتراک‌ها بدون فیلتر تاریخ برای این کاربر
            var allUserSubs = await _context.Subscriptions
                .Where(s => s.UserId.ToString() == rawUserId)
                .ToListAsync(ct);

            var activeSubscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId.ToString() == rawUserId && s.Status == SubscriptionStatus.Active && s.EndDateUtc > now)
                .OrderByDescending(s => s.EndDateUtc)
                .FirstOrDefaultAsync(ct);

            // ارسال مقادیر به ViewBag برای نمایش روی صفحه جهت تست
            ViewBag.RawUserId = rawUserId;
            ViewBag.AllSubsCount = allUserSubs.Count;
            ViewBag.ActiveSubscription = activeSubscription;

            return View();
        }





        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Application.Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
