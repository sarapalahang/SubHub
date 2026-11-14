using Microsoft.EntityFrameworkCore;
using SubHub.Application.Interfaces;
using SubHub.Contracts.Invoices;
using SubHub.Domain.Entities;
using SubHub.Domain.Enums;

namespace SubHub.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly IEmailService _emailService;
    private readonly IPdfService _pdfService;

    public InvoiceService(
        IApplicationDbContext context,
        IPaymentGatewayService paymentGatewayService,
        IEmailService emailService,
        IPdfService pdfService)
    {
        _context = context;
        _paymentGatewayService = paymentGatewayService;
        _emailService = emailService;
        _pdfService = pdfService;
    }

    public async Task<CreateInvoiceResponse> CreateInvoiceAndInitiatePaymentAsync(
        CreateInvoiceRequest request,
        string callbackUrl,
        string? userEmail = null,
        string? userMobile = null,
        CancellationToken cancellationToken = default)
    {
        var plan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive, cancellationToken);

        if (plan is null)
            throw new InvalidOperationException("پلن انتخابی یافت نشد یا غیرفعال است.");

        var user = await _context.Users
            .FirstOrDefaultAsync(candidate => candidate.Id == request.UserId, cancellationToken);
        var submittedEmail = string.IsNullOrWhiteSpace(request.BuyerEmail)
            ? null
            : request.BuyerEmail.Trim();

        if (user is not null && submittedEmail is not null)
            user.Email = submittedEmail;

        var buyerEmail = submittedEmail ?? (string.IsNullOrWhiteSpace(user?.Email) ? null : user.Email.Trim());

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..28];

        var invoice = new Invoice
        {
            UserId = request.UserId,
            PlanId = plan.Id,

            // ذخیره اسنپ‌شات مشخصات خریدار در لحظه خرید
            BuyerName = request.BuyerName,
            BuyerPhoneNumber = request.BuyerPhoneNumber,
            BuyerEmail = buyerEmail,

            InvoiceNumber = invoiceNumber,
            Amount = plan.Price,
            Status = InvoiceStatus.PendingPayment,
            PaymentGateway = _paymentGatewayService.ProviderName
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        var description = $"خرید اشتراک {plan.Title} - {request.BuyerName}";
        var paymentResult = await _paymentGatewayService.RequestPaymentAsync(
            (long)invoice.Amount,
            description,
            callbackUrl,
            userEmail ?? buyerEmail,
            userMobile ?? request.BuyerPhoneNumber);

        if (!paymentResult.IsSuccess || string.IsNullOrEmpty(paymentResult.Authority))
        {
            invoice.Status = InvoiceStatus.Failed;
            await _context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException(paymentResult.ErrorMessage ?? "خطا در اتصال به درگاه پرداخت.");
        }

        invoice.Authority = paymentResult.Authority;
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateInvoiceResponse
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = invoice.Amount,
            Authority = paymentResult.Authority,
            PaymentUrl = paymentResult.PaymentUrl ?? string.Empty
        };
    }

    public async Task<PaymentCallbackResult> VerifyAndProcessPaymentAsync(
        string authority,
        string status,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Plan)
            .FirstOrDefaultAsync(i => i.Authority == authority, cancellationToken);

        if (invoice is null)
        {
            return new PaymentCallbackResult { IsSuccess = false, Message = "فاکتور یافت نشد." };
        }

        var userPhoneNumber = invoice.BuyerPhoneNumber;
        var userEmail = invoice.BuyerEmail;

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return new PaymentCallbackResult
            {
                IsSuccess = true,
                InvoiceNumber = invoice.InvoiceNumber,
                RefId = invoice.RefId,
                UserPhoneNumber = userPhoneNumber,
                Message = "این فاکتور قبلاً پرداخت شده است."
            };
        }

        if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            invoice.Status = InvoiceStatus.Cancelled;
            await _context.SaveChangesAsync(cancellationToken);

            return new PaymentCallbackResult
            {
                IsSuccess = false,
                InvoiceNumber = invoice.InvoiceNumber,
                UserPhoneNumber = userPhoneNumber,
                Message = "پرداخت لغو شد."
            };
        }

        var verifyResult = await _paymentGatewayService.VerifyPaymentAsync(authority, (long)invoice.Amount);

        if (!verifyResult.IsSuccess)
        {
            invoice.Status = InvoiceStatus.Failed;
            await _context.SaveChangesAsync(cancellationToken);
            return new PaymentCallbackResult
            {
                IsSuccess = false,
                InvoiceNumber = invoice.InvoiceNumber,
                UserPhoneNumber = userPhoneNumber,
                Message = verifyResult.ErrorMessage ?? "تأیید پرداخت ناموفق بود."
            };
        }

        var now = DateTime.UtcNow;
        var existingActiveSubscription = await _context.Subscriptions
            .Where(s => s.UserId == invoice.UserId && s.Status == SubscriptionStatus.Active && s.EndDateUtc > now)
            .OrderByDescending(s => s.EndDateUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var startDate = now;
        var endDate = now.AddDays(invoice.Plan.DurationInDays);

        if (existingActiveSubscription is not null)
        {
            startDate = existingActiveSubscription.EndDateUtc;
            endDate = existingActiveSubscription.EndDateUtc.AddDays(invoice.Plan.DurationInDays);
        }

        var subscription = new Subscription
        {
            UserId = invoice.UserId,
            PlanId = invoice.PlanId,
            StartDateUtc = startDate,
            EndDateUtc = endDate,
            Status = SubscriptionStatus.Active
        };

        _context.Subscriptions.Add(subscription);
        invoice.Status = InvoiceStatus.Paid;
        invoice.RefId = verifyResult.RefId;
        invoice.PaidAtUtc = now;

        await _context.SaveChangesAsync(cancellationToken);

        invoice.SubscriptionId = subscription.Id;
        await _context.SaveChangesAsync(cancellationToken);

        // ارسال فایل PDF فاکتور به ایمیل خریدار
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            try
            {
                // ۱. تولید بایت‌های فایل PDF فاکتور
                var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);

                // ۲. ارسال ایمیل با فایل PDF پیوست‌شده (ارسال بایت‌ها، نه لینک)
                await _emailService.SendInvoiceEmailWithPdfAsync(
                    userEmail,
                    invoice.InvoiceNumber,
                    pdfBytes);
            }
            catch (Exception ex)
            {
                // لاگ خطا در صورت بروز مشکل در ارسال ایمیل
                Console.WriteLine($"[EmailError] ارسال ایمیل فاکتور با خطا مواجه شد: {ex.Message}");
            }
        }


        return new PaymentCallbackResult
        {
            IsSuccess = true,
            InvoiceNumber = invoice.InvoiceNumber,
            RefId = invoice.RefId,
            UserPhoneNumber = userPhoneNumber,
            Message = "پرداخت با موفقیت انجام و اشتراک فعال گردید."
        };
    }
    public async Task<int> ReconcilePendingInvoicesAsync(CancellationToken cancellationToken = default)
    {
        var thresholdTime = DateTime.UtcNow.AddMinutes(-15); // فاکتورهایی که حداقل 15 دقیقه گذشته‌اند
        var cutoffTime = DateTime.UtcNow.AddHours(24);       // فاکتورهای قدیمی‌تر از 24 ساعت منقضی می‌شوند

        var pendingInvoices = await _context.Invoices
            .Include(i => i.Plan)
            .Where(i => i.Status == InvoiceStatus.PendingPayment && i.CreatedAtUtc <= thresholdTime)
            .ToListAsync(cancellationToken);

        int reconciledCount = 0;

        foreach (var invoice in pendingInvoices)
        {
            // اگر فاکتور از 24 ساعت گذشته، منقضی در نظر گرفته شود
            if (invoice.CreatedAtUtc < DateTime.UtcNow.AddHours(-24))
            {
                invoice.Status = InvoiceStatus.Expired;
                continue;
            }

            if (string.IsNullOrEmpty(invoice.Authority))
                continue;

            // استعلام از درگاه
            var inquiryResult = await _paymentGatewayService.InquiryPaymentAsync(invoice.Authority);

            if (inquiryResult.IsPaid)
            {
                // اگر در درگاه پرداخت شده بود، فرآیند وریفای و فعال‌سازی اشتراک را اجرا می‌کنیم
                var verifyResult = await _paymentGatewayService.VerifyPaymentAsync(invoice.Authority, (long)invoice.Amount);

                if (verifyResult.IsSuccess)
                {
                    var now = DateTime.UtcNow;
                    var existingActiveSubscription = await _context.Subscriptions
                        .Where(s => s.UserId == invoice.UserId && s.Status == SubscriptionStatus.Active && s.EndDateUtc > now)
                        .OrderByDescending(s => s.EndDateUtc)
                        .FirstOrDefaultAsync(cancellationToken);

                    var startDate = now;
                    var endDate = now.AddDays(invoice.Plan.DurationInDays);

                    if (existingActiveSubscription is not null)
                    {
                        startDate = existingActiveSubscription.EndDateUtc;
                        endDate = existingActiveSubscription.EndDateUtc.AddDays(invoice.Plan.DurationInDays);
                    }

                    var subscription = new Subscription
                    {
                        UserId = invoice.UserId,
                        PlanId = invoice.PlanId,
                        StartDateUtc = startDate,
                        EndDateUtc = endDate,
                        Status = SubscriptionStatus.Active
                    };

                    _context.Subscriptions.Add(subscription);
                    invoice.Status = InvoiceStatus.Paid;
                    invoice.RefId = verifyResult.RefId ?? inquiryResult.RefId;
                    invoice.PaidAtUtc = now;

                    await _context.SaveChangesAsync(cancellationToken);

                    invoice.SubscriptionId = subscription.Id;
                    await _context.SaveChangesAsync(cancellationToken);

                    // ارسال ایمیل فاکتور
                    if (!string.IsNullOrWhiteSpace(invoice.BuyerEmail))
                    {
                        try
                        {
                            var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);
                            await _emailService.SendInvoiceEmailWithPdfAsync(invoice.BuyerEmail, invoice.InvoiceNumber, pdfBytes);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ReconcileEmailError] {ex.Message}");
                        }
                    }

                    reconciledCount++;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return reconciledCount;
    }

}
