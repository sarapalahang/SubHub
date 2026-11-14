using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SubHub.Application.Interfaces;
using SubHub.Contracts.Invoices;
using SubHub.Domain.Entities;

namespace SubHub.Web.Controllers;

public class PaymentController : Controller
{
    private readonly IInvoiceService _invoiceService;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly IPlanService _planService;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService; // اضافه شد
    private readonly IPdfService _pdfService;     // اضافه شد
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;

    public PaymentController(
        IInvoiceService invoiceService,
        IPaymentGatewayService paymentGatewayService,
        IPlanService planService,
        ISmsService smsService,
        IEmailService emailService, // اضافه شد
        IPdfService pdfService,     // اضافه شد
        IConfiguration configuration,
        IApplicationDbContext context)
    {
        _invoiceService = invoiceService;
        _paymentGatewayService = paymentGatewayService;
        _planService = planService;
        _smsService = smsService;
        _emailService = emailService;
        _pdfService = pdfService;
        _configuration = configuration;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Preview(long planId)
    {
        var plans = await _planService.GetActivePlansAsync(CancellationToken.None);
        var selectedPlan = plans.FirstOrDefault(p => p.Id == planId);

        if (selectedPlan == null)
        {
            TempData["ErrorMessage"] = "پلن انتخابی یافت نشد.";
            return RedirectToAction("Index", "Plan");
        }

        return View(selectedPlan);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(
    long planId,
    string buyerName,
    string buyerPhoneNumber,
    string? buyerEmail)
    {
        buyerName = buyerName?.Trim() ?? string.Empty;
        buyerPhoneNumber = buyerPhoneNumber?.Trim() ?? string.Empty;
        buyerEmail = buyerEmail?.Trim();

        if (!string.IsNullOrWhiteSpace(buyerEmail) &&
            (!System.Net.Mail.MailAddress.TryCreate(buyerEmail, out var parsedEmail) ||
             !string.Equals(parsedEmail.Address, buyerEmail, StringComparison.OrdinalIgnoreCase)))
        {
            TempData["ErrorMessage"] = "نشانی ایمیل واردشده معتبر نیست.";
            return RedirectToAction("Preview", new { planId });
        }

        if (string.IsNullOrWhiteSpace(buyerName) ||
            string.IsNullOrWhiteSpace(buyerPhoneNumber))
        {
            TempData["ErrorMessage"] = "نام و شماره موبایل را وارد کنید.";
            return RedirectToAction("Preview", new { planId });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == buyerPhoneNumber);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = buyerName,
                PhoneNumber = buyerPhoneNumber
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var callbackUrl = _configuration["ZarinPal:CallbackUrl"];

        if (string.IsNullOrWhiteSpace(callbackUrl))
        {
            TempData["ErrorMessage"] = "آدرس بازگشت درگاه پرداخت تنظیم نشده است.";
            return RedirectToAction("Preview", new { planId });
        }

        var request = new CreateInvoiceRequest
        {
            UserId = user.Id,
            PlanId = planId,
            BuyerName = buyerName,
            BuyerPhoneNumber = buyerPhoneNumber,
            BuyerEmail = buyerEmail
        };

        try
        {
            var result = await _invoiceService.CreateInvoiceAndInitiatePaymentAsync(
                request,
                callbackUrl,
                userEmail: buyerEmail,
                userMobile: buyerPhoneNumber,
                cancellationToken: CancellationToken.None);

            return Redirect(result.PaymentUrl);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Preview", new { planId });
        }
    }


    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Checkout(
    //    long planId,
    //    string buyerName,
    //    string buyerPhoneNumber,
    //    string? buyerEmail)
    //{
    //    buyerName = buyerName?.Trim() ?? string.Empty;
    //    buyerPhoneNumber = buyerPhoneNumber?.Trim() ?? string.Empty;
    //    buyerEmail = buyerEmail?.Trim();

    //    if (string.IsNullOrWhiteSpace(buyerName) ||
    //        string.IsNullOrWhiteSpace(buyerPhoneNumber))
    //    {
    //        TempData["ErrorMessage"] = "نام و شماره موبایل را وارد کنید.";
    //        return RedirectToAction("Preview", new { planId });
    //    }

    //    var user = await _context.Users
    //        .FirstOrDefaultAsync(u => u.PhoneNumber == buyerPhoneNumber);

    //    if (user == null)
    //    {
    //        user = new User
    //        {
    //            Id = Guid.NewGuid(),
    //            Name = buyerName,
    //            PhoneNumber = buyerPhoneNumber
    //        };

    //        _context.Users.Add(user);
    //        await _context.SaveChangesAsync();
    //    }

    //    var callbackUrl = _configuration["ZarinPal:CallbackUrl"];

    //    if (string.IsNullOrWhiteSpace(callbackUrl))
    //    {
    //        TempData["ErrorMessage"] = "آدرس بازگشت درگاه پرداخت تنظیم نشده است.";
    //        return RedirectToAction("Preview", new { planId });
    //    }

    //    var request = new CreateInvoiceRequest
    //    {
    //        UserId = user.Id,
    //        PlanId = planId,
    //        BuyerName = buyerName,
    //        BuyerPhoneNumber = buyerPhoneNumber,
    //        BuyerEmail = buyerEmail
    //    };

    //    try
    //    {
    //        var result = await _invoiceService.CreateInvoiceAndInitiatePaymentAsync(
    //            request,
    //            callbackUrl,
    //            userEmail: buyerEmail,
    //            userMobile: buyerPhoneNumber,
    //            cancellationToken: CancellationToken.None);

    //        return Redirect(result.PaymentUrl);
    //    }
    //    catch (Exception ex)
    //    {
    //        TempData["ErrorMessage"] = ex.Message;
    //        return RedirectToAction("Preview", new { planId });
    //    }
    //}


    //[HttpGet]
    //public async Task<IActionResult> Callback(string authority, string status)
    //{
    //    var result = await _invoiceService.VerifyAndProcessPaymentAsync(authority, status);

    //    if (result.IsSuccess)
    //    {
    //        // خواندن تنظیمات مدیر سایت (سوییچ‌های پیامک و ایمیل)
    //        var settings = await _context.SiteSettings.FirstOrDefaultAsync() ?? new SiteSetting();

    //        // پیدا کردن فاکتور برای دسترسی به مشخصات خریدار
    //        var invoice = await _context.Invoices
    //            .Include(i => i.Plan)
    //            .FirstOrDefaultAsync(i => i.InvoiceNumber == result.InvoiceNumber || i.Id == result.InvoiceId);

    //        if (invoice != null)
    //        {
    //            // ۱. ارسال پیامک (بدون تغییر)
    //            if (settings.EnableSmsNotification && !string.IsNullOrEmpty(invoice.BuyerPhoneNumber))
    //            {
    //                try
    //                {
    //                    string customerName = string.IsNullOrWhiteSpace(invoice.BuyerName) ? "مشتری گرامی" : invoice.BuyerName;
    //                    string smsMessage = $"{customerName} عزیز، اشتراک شما ({invoice.Plan?.Name ?? "پلن"}) با موفقیت فعال شد.";
    //                    await _smsService.SendSmsAsync(invoice.BuyerPhoneNumber, smsMessage);
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine("[SMS Error]: " + ex.Message);
    //                }
    //            }

    //            // ۲. ارسال مستقیم ایمیل (بدون گیر کردن در خطای PDF)
    //            if (settings.EnableEmailNotification && !string.IsNullOrEmpty(invoice.BuyerEmail))
    //            {
    //                try
    //                {
    //                    Console.WriteLine(">>> در حال ارسال ایمیل فاکتور به: " + invoice.BuyerEmail);
    //                    await _emailService.SendInvoiceEmailWithPdfAsync(
    //                        invoice.BuyerEmail,
    //                        invoice.InvoiceNumber,
    //                        null);

    //                    Console.WriteLine(">>> ایمیل فاکتور با موفقیت ارسال شد.");
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine("**************************************");
    //                    Console.WriteLine("[CRITICAL EMAIL ERROR]: " + ex.Message);
    //                    if (ex.InnerException != null)
    //                    {
    //                        Console.WriteLine("[Inner Error]: " + ex.InnerException.Message);
    //                    }
    //                    Console.WriteLine("STACK TRACE: " + ex.StackTrace);
    //                    Console.WriteLine("**************************************");
    //                }
    //            }
    //        }

    //        return View("Success", result);
    //    }
    //    else
    //    {
    //        return View("Failure");
    //    }
    //}
    [HttpGet]
    public async Task<IActionResult> Callback(string authority, string status)
    {
        var result = await _invoiceService.VerifyAndProcessPaymentAsync(authority, status);

        if (result.IsSuccess)
        {
            // خواندن تنظیمات مدیر سایت (سوییچ‌های پیامک و ایمیل)
            var settings = await _context.SiteSettings.FirstOrDefaultAsync() ?? new SiteSetting();

            // پیدا کردن فاکتور برای دسترسی به مشخصات خریدار
            var invoice = await _context.Invoices
                .Include(i => i.Plan)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == result.InvoiceNumber || i.Id == result.InvoiceId);

            if (invoice != null)
            {
                // ۱. ارسال پیامک (بدون تغییر)
                if (settings.EnableSmsNotification && !string.IsNullOrEmpty(invoice.BuyerPhoneNumber))
                {
                    try
                    {
                        string customerName = string.IsNullOrWhiteSpace(invoice.BuyerName) ? "مشتری گرامی" : invoice.BuyerName;
                        string smsMessage = $"{customerName} عزیز، اشتراک شما ({invoice.Plan?.Name ?? "پلن"}) با موفقیت فعال شد.";
                        await _smsService.SendSmsAsync(invoice.BuyerPhoneNumber, smsMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[SMS Error]: " + ex.Message);
                    }
                }

                // ۲. ارسال ایمیل و تولید ایمن PDF
                if (settings.EnableEmailNotification && !string.IsNullOrEmpty(invoice.BuyerEmail))
                {
                    try
                    {
                        Console.WriteLine(">>> در حال آماده‌سازی ایمیل فاکتور برای: " + invoice.BuyerEmail);

                        byte[]? pdfBytes = null;

                        // تلاش برای ساخت PDF به صورت کاملاً ایزوله (اگر خطا داد برنامه کرش نکند)
                        if (settings.EnablePdfAttachment && _pdfService != null)
                        {
                            try
                            {
                                pdfBytes = _pdfService.GenerateInvoicePdf(invoice);
                            }
                            catch (Exception pdfEx)
                            {
                                Console.WriteLine("[PDF Generation Warning]: خطا در ساخت PDF، ارسال ایمیل بدون پیوست انجام می‌شود. جزئیات: " + pdfEx.Message);
                            }
                        }

                        // ارسال ایمیل (اگر PDF ساخته شده باشد ضمیمه می‌شود، وگرنه فقط متن می‌رود)
                        await _emailService.SendInvoiceEmailWithPdfAsync(
                            invoice.BuyerEmail,
                            invoice.InvoiceNumber,
                            pdfBytes);

                        Console.WriteLine(">>> ایمیل فاکتور با موفقیت ارسال شد.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("**************************************");
                        Console.WriteLine("[CRITICAL EMAIL ERROR]: " + ex.Message);
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine("[Inner Error]: " + ex.InnerException.Message);
                        }
                        Console.WriteLine("STACK TRACE: " + ex.StackTrace);
                        Console.WriteLine("**************************************");
                    }
                }
            }

            return View("Success", result);
        }
        else
        {
            return View("Failure");
        }
    }
}
