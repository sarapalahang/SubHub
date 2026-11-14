using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubHub.Application.Interfaces;
using SubHub.Domain.Enums;
using SubHub.Web.Models;
using System.Security.Claims;


namespace SubHub.Web.Controllers;

[Authorize(AuthenticationSchemes = "SubHubCookie")]
public class InvoiceController : Controller
{
    private readonly IApplicationDbContext _context;

    public InvoiceController(IApplicationDbContext context)
    {
        _context = context;
    }

    // آدرس: /Invoice/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Forbid();

        var invoice = await _context.Invoices
            .Include(i => i.Plan)
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId.Value);

        if (invoice == null)
        {
            return NotFound("فاکتور مورد نظر یافت نشد.");
        }

        return View(invoice);
    }
    [HttpGet]
    public async Task<IActionResult> PurchaseHistory()
    {
        // ۱. بررسی اینکه آیا کاربر لاگین کرده است یا خیر
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");  // یا نام کنترلر لاگین خودتان (مثل Account)
        }

        // ۲. گرفتن شناسه کاربر جاری
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
        {
            return RedirectToAction("Login", "Auth");
        }

        // ۳. فیلتر کردن فاکتورها بر اساس شناسه کاربر لاگین‌شده
        var invoices = await _context.Invoices
            .Include(i => i.Plan)
            .Where(i => i.UserId == currentUserId)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync();

        var model = new PurchaseHistoryViewModel
        {
            Items = invoices.Select(i => new PurchaseHistoryItemViewModel
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                PlanName = i.Plan?.Name ?? "نامشخص",
                Amount = i.Amount,
                StatusText = i.Status switch
                {
                    InvoiceStatus.Paid => "پرداخت شده",
                    InvoiceStatus.PendingPayment => "در انتظار پرداخت",
                    InvoiceStatus.Pending => "در انتظار بررسی",
                    InvoiceStatus.Cancelled => "لغو شده",
                    InvoiceStatus.Failed => "ناموفق",
                    InvoiceStatus.Expired => "منقضی شده",
                    _ => "نامشخص"
                },
                StatusBadgeClass = i.Status switch
                {
                    InvoiceStatus.Paid => "bg-success",
                    InvoiceStatus.PendingPayment => "bg-warning text-dark",
                    InvoiceStatus.Pending => "bg-info text-dark",
                    InvoiceStatus.Cancelled => "bg-secondary",
                    InvoiceStatus.Failed => "bg-danger",
                    InvoiceStatus.Expired => "bg-danger",
                    _ => "bg-secondary"
                },
                CreatedAtUtc = i.CreatedAtUtc
            }).ToList()
        };

        return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> DownloadPdf(Guid id, [FromServices] IPdfService pdfService)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Forbid();

        var invoice = await _context.Invoices
            .Include(i => i.Plan)
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId.Value);

        if (invoice == null)
        {
            return NotFound("فاکتور مورد نظر یافت نشد.");
        }

        var pdfBytes = pdfService.GenerateInvoicePdf(invoice);
        return File(pdfBytes, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }

}
