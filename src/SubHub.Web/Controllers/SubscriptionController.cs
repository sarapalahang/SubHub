using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubHub.Application.Interfaces;
using SubHub.Domain.Enums;
using SubHub.Web.Models;
using System.Security.Claims;

namespace SubHub.Web.Controllers;

[Authorize(AuthenticationSchemes = "SubHubCookie")]
public class SubscriptionController : Controller
{
    private readonly IApplicationDbContext _context;

    public SubscriptionController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Current(CancellationToken cancellationToken)
    {
        // گرفتن شناسه کاربر لاگین‌شده
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Account");

        // 1) تلاش: آخرین اشتراک Active کاربر (به همراه Plan)
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartDateUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
        {
            ViewBag.Message = "هیچ اشتراک فعالی برای شما ثبت نشده است.";
            return View("CurrentEmpty");
        }

        // 2) پیدا کردن آخرین فاکتور Paid مرتبط با این اشتراک (اختیاری)
        var invoice = await _context.Invoices
            .Include(i => i.Plan)
            .Where(i => i.UserId == userId &&
                        i.SubscriptionId == subscription.Id &&
                        i.Status == SubHub.Domain.Enums.InvoiceStatus.Paid)
            .OrderByDescending(i => i.PaidAtUtc ?? i.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        // مدل ویو ساده
        var model = new CurrentSubscriptionViewModel
        {
            SubscriptionId = subscription.Id,
            PlanName = subscription.Plan?.Name ?? subscription.Plan?.Title ?? "نامشخص",
            PlanTitle = subscription.Plan?.Title,
            StartDateUtc = subscription.StartDateUtc,
            EndDateUtc = subscription.EndDateUtc,
            Status = subscription.Status,
            IsActiveNow = subscription.IsActiveNow,
            CanCancel = subscription.Status == SubscriptionStatus.Active && subscription.IsActiveNow,
            CancelledAtUtc = subscription.CancelledAtUtc,
            CancellationReason = subscription.CancellationReason,
            CancellationReasonDetails = subscription.CancellationReasonDetails,
            Amount = invoice?.Amount,
            InvoiceNumber = invoice?.InvoiceNumber,
            PaidAtUtc = invoice?.PaidAtUtc
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(
        long subscriptionId,
        SubscriptionCancellationReason? reason,
        string? reasonDetails,
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Account");

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(
                item => item.Id == subscriptionId && item.UserId == userId,
                cancellationToken);

        if (subscription is null)
            return NotFound();

        if (subscription.Status != SubscriptionStatus.Active || !subscription.IsActiveNow)
        {
            TempData["ErrorMessage"] = "این اشتراک در حال حاضر قابل فسخ نیست.";
            return RedirectToAction(nameof(Current));
        }

        var details = reasonDetails?.Trim();
        if (reason is null ||
            !Enum.IsDefined(reason.Value) ||
            (reason == SubscriptionCancellationReason.Other && string.IsNullOrWhiteSpace(details)) ||
            details?.Length > 500)
        {
            TempData["ErrorMessage"] = "لطفاً دلیل معتبر انتخاب کنید و برای «دلیل دیگر» توضیح بنویسید (حداکثر ۵۰۰ نویسه).";
            return RedirectToAction(nameof(Current));
        }

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.CancelledAtUtc = DateTime.UtcNow;
        subscription.CancellationReason = reason.Value;
        subscription.CancellationReasonDetails = string.IsNullOrWhiteSpace(details) ? null : details;

        await _context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "اشتراک شما فسخ شد.";
        return RedirectToAction(nameof(Current));
    }
}

