using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubHub.Application.Interfaces;
using SubHub.Domain.Enums;

namespace SubHub.Web.Controllers
{
    public class PlanController : Controller
    {
        private readonly IPlanService _planService;
        private readonly IApplicationDbContext _context;

        public PlanController(IPlanService planService, IApplicationDbContext context)
        {
            _planService = planService;
            _context = context;
        }

        // GET: /Plan
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var plans = await _planService.GetActivePlansAsync(ct);

            // پیدا کردن آخرین اشتراک فعال کاربر برای تعیین بسته فعال
            var now = DateTime.UtcNow;
            var activeSubscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.Status == SubscriptionStatus.Active && s.EndDateUtc > now)
                .OrderByDescending(s => s.EndDateUtc)
                .FirstOrDefaultAsync(ct);

            ViewBag.ActivePlanId = activeSubscription?.PlanId;

            return View(plans);
        }

    }
}
