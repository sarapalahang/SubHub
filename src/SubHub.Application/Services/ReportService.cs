using Microsoft.EntityFrameworkCore;
using SubHub.Application.Interfaces;
using SubHub.Contracts.Reports;
using SubHub.Domain.Enums;

namespace SubHub.Application.Services;

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;

    public ReportService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardReportDto> GetDashboardReportAsync(CancellationToken cancellationToken = default)
    {
        var report = new DashboardReportDto();
        var now = DateTime.UtcNow;

        report.TotalRevenue = await _context.Invoices
            .Where(invoice => invoice.Status == InvoiceStatus.Paid)
            .SumAsync(invoice => invoice.Amount, cancellationToken);

        report.TotalSubscribers = await _context.Users.CountAsync(cancellationToken);
        report.TotalSubscriptions = await _context.Subscriptions.CountAsync(cancellationToken);
        report.CancelledSubscriptionsCount = await _context.Subscriptions
            .CountAsync(subscription => subscription.Status == SubscriptionStatus.Cancelled, cancellationToken);
        report.CancellationRatePercentage = report.TotalSubscriptions == 0
            ? 0
            : Math.Round(report.CancelledSubscriptionsCount * 100m / report.TotalSubscriptions, 2);

        var reasonCounts = await _context.Subscriptions
            .Where(subscription => subscription.Status == SubscriptionStatus.Cancelled)
            .GroupBy(subscription => subscription.CancellationReason)
            .Select(group => new { Reason = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        report.CancellationReasons = reasonCounts
            .Select(item => new CancellationReasonReportDto
            {
                Reason = item.Reason?.ToPersianLabel() ?? "ثبت نشده",
                Count = item.Count
            })
            .OrderByDescending(item => item.Count)
            .ToList();

        var recentCancellations = await _context.Subscriptions
            .Where(subscription => subscription.Status == SubscriptionStatus.Cancelled)
            .OrderByDescending(subscription => subscription.CancelledAtUtc)
            .Take(10)
            .Select(subscription => new
            {
                subscription.Plan.Title,
                subscription.CancellationReason,
                subscription.CancellationReasonDetails,
                subscription.CancelledAtUtc
            })
            .ToListAsync(cancellationToken);

        report.RecentCancellations = recentCancellations
            .Select(item => new RecentCancellationDto
            {
                PlanTitle = item.Title,
                Reason = item.CancellationReason?.ToPersianLabel() ?? "ثبت نشده",
                Details = item.CancellationReasonDetails,
                CancelledAtUtc = item.CancelledAtUtc
            })
            .ToList();

        report.ActiveSubscriptions = await _context.Subscriptions.CountAsync(
            subscription => subscription.Status == SubscriptionStatus.Active && subscription.EndDateUtc >= now,
            cancellationToken);

        report.PendingInvoicesCount = await _context.Invoices.CountAsync(
            invoice => invoice.Status == InvoiceStatus.Pending || invoice.Status == InvoiceStatus.PendingPayment,
            cancellationToken);

        var sixMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);
        var paidInvoices = await _context.Invoices
            .Where(invoice => invoice.Status == InvoiceStatus.Paid &&
                (invoice.PaidAtUtc ?? invoice.CreatedAtUtc) >= sixMonthStart &&
                (invoice.PaidAtUtc ?? invoice.CreatedAtUtc) <= now)
            .Select(invoice => new
            {
                RevenueAtUtc = invoice.PaidAtUtc ?? invoice.CreatedAtUtc,
                invoice.Amount
            })
            .ToListAsync(cancellationToken);

        var cancellationDates = await _context.Subscriptions
            .Where(subscription => subscription.Status == SubscriptionStatus.Cancelled &&
                subscription.CancelledAtUtc >= sixMonthStart &&
                subscription.CancelledAtUtc <= now)
            .Select(subscription => subscription.CancelledAtUtc!.Value)
            .ToListAsync(cancellationToken);

        for (var offset = 5; offset >= 0; offset--)
        {
            var month = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-offset);
            report.MonthlyLabels.Add(month.ToString("yyyy/MM"));
            report.MonthlyRevenues.Add(paidInvoices
                .Where(item => item.RevenueAtUtc.Year == month.Year && item.RevenueAtUtc.Month == month.Month)
                .Sum(item => item.Amount));
            report.MonthlyCancelledSubscriptions.Add(cancellationDates.Count(date =>
                date.Year == month.Year && date.Month == month.Month));
        }

        report.RecentInvoices = await _context.Invoices
            .OrderByDescending(invoice => invoice.CreatedAtUtc)
            .Take(5)
            .Select(invoice => new RecentInvoiceDto
            {
                Id = invoice.Id,
                UserFullName = (invoice.User != null ? invoice.User.Name : invoice.BuyerName) ?? "کاربر ناشناس",
                PlanTitle = invoice.Plan != null ? invoice.Plan.Title : "پلن نامشخص",
                Amount = invoice.Amount,
                Status = invoice.Status.ToString(),
                CreatedAt = invoice.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return report;
    }

    public async Task<SubscriptionCommerceReportDto> GetSubscriptionCommerceReportAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentDayStart = now.Date;
        // هفتهٔ گزارش از شنبه (روز شروع هفته در تقویم ایران) محاسبه می‌شود.
        var currentWeekStart = currentDayStart.AddDays(-(((int)currentDayStart.DayOfWeek + 1) % 7));
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var currentYearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var annualStart = new DateTime(now.Year - 4, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var planBreakdown = await _context.Invoices
            .Where(invoice => invoice.Status == InvoiceStatus.Paid)
            .GroupBy(invoice => new { invoice.PlanId, invoice.Plan.Title })
            .Select(group => new PlanCommerceBreakdownDto
            {
                PlanId = group.Key.PlanId,
                PlanTitle = group.Key.Title,
                SalesCount = group.Count(),
                Revenue = group.Sum(invoice => invoice.Amount)
            })
            .OrderByDescending(item => item.SalesCount)
            .ToListAsync(cancellationToken);

        var durationGroups = await _context.Invoices
            .Where(invoice => invoice.Status == InvoiceStatus.Paid)
            .GroupBy(invoice => new { invoice.Plan.DurationType, invoice.Plan.DurationInDays })
            .Select(group => new
            {
                group.Key.DurationType,
                group.Key.DurationInDays,
                SalesCount = group.Count(),
                Revenue = group.Sum(invoice => invoice.Amount)
            })
            .ToListAsync(cancellationToken);

        var gatewayBreakdown = await _context.Invoices
            .Where(invoice => invoice.Status == InvoiceStatus.Paid)
            .GroupBy(invoice => invoice.PaymentGateway)
            .Select(group => new GatewayRevenueDto
            {
                GatewayName = group.Key,
                SalesCount = group.Count(),
                Revenue = group.Sum(invoice => invoice.Amount)
            })
            .OrderByDescending(item => item.Revenue)
            .ToListAsync(cancellationToken);

        var paidInvoices = await _context.Invoices
            .Where(invoice => invoice.Status == InvoiceStatus.Paid &&
                (invoice.PaidAtUtc ?? invoice.CreatedAtUtc) >= annualStart &&
                (invoice.PaidAtUtc ?? invoice.CreatedAtUtc) <= now)
            .Select(invoice => new
            {
                SaleDateUtc = invoice.PaidAtUtc ?? invoice.CreatedAtUtc,
                invoice.Amount
            })
            .ToListAsync(cancellationToken);

        var report = new SubscriptionCommerceReportDto
        {
            TotalSales = planBreakdown.Sum(item => item.SalesCount),
            TotalRevenue = planBreakdown.Sum(item => item.Revenue),
            TodaySales = paidInvoices.Count(item => item.SaleDateUtc >= currentDayStart),
            ThisWeekSales = paidInvoices.Count(item => item.SaleDateUtc >= currentWeekStart),
            CurrentMonthSales = paidInvoices.Count(item => item.SaleDateUtc >= currentMonthStart),
            PreviousMonthSales = paidInvoices.Count(item =>
                item.SaleDateUtc >= previousMonthStart && item.SaleDateUtc < currentMonthStart),
            CurrentYearSales = paidInvoices.Count(item => item.SaleDateUtc >= currentYearStart),
            CurrentMonthRevenue = paidInvoices.Where(item => item.SaleDateUtc >= currentMonthStart).Sum(item => item.Amount),
            PreviousMonthRevenue = paidInvoices
                .Where(item => item.SaleDateUtc >= previousMonthStart && item.SaleDateUtc < currentMonthStart)
                .Sum(item => item.Amount),
            CurrentYearRevenue = paidInvoices.Where(item => item.SaleDateUtc >= currentYearStart).Sum(item => item.Amount),
            PlanBreakdown = planBreakdown,
            PlanDurationBreakdown = durationGroups
                .Select(group => new PlanDurationRevenueDto
                {
                    DurationLabel = GetDurationLabel(group.DurationType, group.DurationInDays),
                    SalesCount = group.SalesCount,
                    Revenue = group.Revenue
                })
                .OrderByDescending(item => item.Revenue)
                .ToList(),
            GatewayBreakdown = gatewayBreakdown
        };

        for (var dayOffset = 6; dayOffset >= 0; dayOffset--)
        {
            var dayStart = currentDayStart.AddDays(-dayOffset);
            report.DailyLabels.Add(dayStart.ToString("MM/dd"));
            report.DailySales.Add(paidInvoices.Count(item =>
                item.SaleDateUtc >= dayStart && item.SaleDateUtc < dayStart.AddDays(1)));
        }

        for (var weekOffset = 7; weekOffset >= 0; weekOffset--)
        {
            var weekStart = currentWeekStart.AddDays(-7 * weekOffset);
            var weekEnd = weekStart.AddDays(7);
            report.WeeklyLabels.Add($"{weekStart:MM/dd}–{weekEnd.AddDays(-1):MM/dd}");
            report.WeeklySales.Add(paidInvoices.Count(item =>
                item.SaleDateUtc >= weekStart && item.SaleDateUtc < weekEnd));
        }

        for (var monthOffset = 5; monthOffset >= 0; monthOffset--)
        {
            var monthStart = currentMonthStart.AddMonths(-monthOffset);
            var nextMonthStart = monthStart.AddMonths(1);
            var monthInvoices = paidInvoices.Where(item =>
                item.SaleDateUtc >= monthStart && item.SaleDateUtc < nextMonthStart).ToList();

            report.MonthlyLabels.Add(monthStart.ToString("yyyy/MM"));
            report.MonthlySales.Add(monthInvoices.Count);
            report.MonthlyRevenue.Add(monthInvoices.Sum(item => item.Amount));
        }

        for (var yearOffset = 4; yearOffset >= 0; yearOffset--)
        {
            var year = now.Year - yearOffset;
            var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            report.AnnualLabels.Add(year.ToString());
            report.AnnualSales.Add(paidInvoices.Count(item => item.SaleDateUtc >= yearStart && item.SaleDateUtc < yearStart.AddYears(1)));
        }

        return report;
    }

    public async Task<ActiveUsersByPlanReportDto> GetActiveUsersByPlanReportAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var firstMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);
        var subscriptions = await _context.Subscriptions
            .Where(subscription =>
                subscription.Status != SubscriptionStatus.Suspended &&
                subscription.StartDateUtc <= now &&
                subscription.EndDateUtc >= firstMonthStart)
            .Select(subscription => new
            {
                subscription.UserId,
                subscription.PlanId,
                PlanTitle = subscription.Plan.Title,
                subscription.StartDateUtc,
                subscription.EndDateUtc,
                subscription.CancelledAtUtc,
                subscription.Status
            })
            .ToListAsync(cancellationToken);

        var monthPoints = Enumerable.Range(0, 6)
            .Select(offset =>
            {
                var start = firstMonthStart.AddMonths(offset);
                var monthEnd = start.AddMonths(1).AddTicks(-1);
                return new
                {
                    Start = start,
                    AsOf = offset == 5 && now < monthEnd ? now : monthEnd
                };
            })
            .ToList();

        var grouped = subscriptions
            .GroupBy(subscription => new { subscription.PlanId, subscription.PlanTitle })
            .Select(group => new ActiveUsersPerPlanDto
            {
                PlanId = group.Key.PlanId,
                PlanTitle = group.Key.PlanTitle,
                ActiveUsers = group.Where(subscription =>
                        subscription.Status == SubscriptionStatus.Active &&
                        subscription.EndDateUtc >= now)
                    .Select(subscription => subscription.UserId)
                    .Distinct()
                    .Count(),
                ActiveSubscriptions = group.Count(subscription =>
                    subscription.Status == SubscriptionStatus.Active &&
                    subscription.EndDateUtc >= now),
                HistoricalActiveUsers = monthPoints.Select(point => group
                    .Where(subscription =>
                        subscription.StartDateUtc <= point.AsOf &&
                        subscription.EndDateUtc >= point.AsOf &&
                        (subscription.CancelledAtUtc == null || subscription.CancelledAtUtc > point.AsOf))
                    .Select(subscription => subscription.UserId)
                    .Distinct()
                    .Count()).ToList()
            })
            .OrderByDescending(plan => plan.ActiveUsers)
            .ThenBy(plan => plan.PlanTitle)
            .ToList();

        var activeNow = subscriptions.Where(subscription =>
            subscription.Status == SubscriptionStatus.Active && subscription.EndDateUtc >= now).ToList();

        return new ActiveUsersByPlanReportDto
        {
            TotalActiveUsers = activeNow.Select(subscription => subscription.UserId).Distinct().Count(),
            TotalActiveSubscriptions = activeNow.Count,
            HistoricalLabels = monthPoints.Select(point => point.Start.ToString("yyyy/MM")).ToList(),
            Plans = grouped
        };
    }

    public async Task<CustomerActivityReportDto> GetCustomerActivityReportAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstMonthStart = currentMonthStart.AddMonths(-5);

        var registrations = await _context.Users
            .Where(user => user.CreatedAtUtc.HasValue &&
                user.CreatedAtUtc.Value >= firstMonthStart &&
                user.CreatedAtUtc.Value <= now)
            .Select(user => new { user.Id, CreatedAtUtc = user.CreatedAtUtc!.Value })
            .ToListAsync(cancellationToken);

        var firstPurchaseDates = await _context.Invoices
            .Where(invoice => invoice.Status == InvoiceStatus.Paid)
            .GroupBy(invoice => invoice.UserId)
            .Select(group => new
            {
                UserId = group.Key,
                FirstPurchaseUtc = group.Min(invoice => invoice.PaidAtUtc ?? invoice.CreatedAtUtc)
            })
            .ToDictionaryAsync(item => item.UserId, item => item.FirstPurchaseUtc, cancellationToken);

        var periodPurchases = await _context.Invoices
            .Where(invoice => invoice.Status == InvoiceStatus.Paid &&
                (invoice.PaidAtUtc ?? invoice.CreatedAtUtc) >= firstMonthStart &&
                (invoice.PaidAtUtc ?? invoice.CreatedAtUtc) <= now)
            .Select(invoice => new
            {
                invoice.UserId,
                SaleDateUtc = invoice.PaidAtUtc ?? invoice.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var report = new CustomerActivityReportDto();
        for (var monthOffset = 5; monthOffset >= 0; monthOffset--)
        {
            var monthStart = currentMonthStart.AddMonths(-monthOffset);
            var nextMonth = monthStart.AddMonths(1);

            var registrationCount = registrations.Count(item =>
                item.CreatedAtUtc >= monthStart && item.CreatedAtUtc < nextMonth);

            var firstTimeBuyerCount = firstPurchaseDates.Count(item =>
                item.Value >= monthStart && item.Value < nextMonth && item.Value <= now);

            var renewalCustomers = periodPurchases
                .Where(item => item.SaleDateUtc >= monthStart && item.SaleDateUtc < nextMonth &&
                    firstPurchaseDates.TryGetValue(item.UserId, out var firstDate) && firstDate < monthStart)
                .Select(item => item.UserId)
                .Distinct()
                .Count();

            var payingCustomerCount = firstTimeBuyerCount + renewalCustomers;
            report.MonthlyLabels.Add(monthStart.ToString("yyyy/MM"));
            report.NewRegistrations.Add(registrationCount);
            report.FirstTimeBuyers.Add(firstTimeBuyerCount);
            report.RenewalCustomers.Add(renewalCustomers);
            report.FirstTimeBuyerPercentages.Add(payingCustomerCount == 0
                ? 0
                : Math.Round(firstTimeBuyerCount * 100m / payingCustomerCount, 1));
            report.RenewalPercentages.Add(payingCustomerCount == 0
                ? 0
                : Math.Round(renewalCustomers * 100m / payingCustomerCount, 1));

            if (monthStart == currentMonthStart)
            {
                report.RegistrationsThisMonth = registrationCount;
                report.FirstTimeBuyersThisMonth = firstTimeBuyerCount;
                report.RenewalCustomersThisMonth = renewalCustomers;
            }
        }

        return report;
    }

    public async Task<PaymentStatusReportDto> GetPaymentStatusReportAsync(
        CancellationToken cancellationToken = default)
    {
        var statusGroups = await _context.Invoices
            .GroupBy(invoice => invoice.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count(),
                Amount = group.Sum(invoice => invoice.Amount)
            })
            .ToListAsync(cancellationToken);

        var byStatus = statusGroups.ToDictionary(item => item.Status);
        PaymentStatusBreakdownDto Create(InvoiceStatus status, string label)
        {
            byStatus.TryGetValue(status, out var item);
            return new PaymentStatusBreakdownDto
            {
                StatusCode = status.ToString(),
                StatusLabel = label,
                Count = item?.Count ?? 0,
                Amount = item?.Amount ?? 0
            };
        }

        var pendingCount = statusGroups
            .Where(item => item.Status is InvoiceStatus.Pending or InvoiceStatus.PendingPayment)
            .Sum(item => item.Count);
        var pendingAmount = statusGroups
            .Where(item => item.Status is InvoiceStatus.Pending or InvoiceStatus.PendingPayment)
            .Sum(item => item.Amount);

        var statuses = new List<PaymentStatusBreakdownDto>
        {
            Create(InvoiceStatus.Paid, "موفق"),
            Create(InvoiceStatus.Failed, "ناموفق"),
            Create(InvoiceStatus.Cancelled, "لغوشده"),
            new() { StatusCode = "Pending", StatusLabel = "در انتظار", Count = pendingCount, Amount = pendingAmount },
            Create(InvoiceStatus.Expired, "منقضی‌شده")
        };

        return new PaymentStatusReportDto
        {
            TotalPayments = statusGroups.Sum(item => item.Count),
            TotalAmount = statusGroups.Sum(item => item.Amount),
            Statuses = statuses
        };
    }

    public async Task<ExpiryNotificationReportDto> GetExpiryNotificationReportAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var sixMonthStart = currentMonthStart.AddMonths(-5);

        var allGroups = await _context.ExpiryNotificationLogs
            .GroupBy(log => new { log.Channel, log.Status })
            .Select(group => new
            {
                group.Key.Channel,
                group.Key.Status,
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);

        int Count(NotificationChannel channel, NotificationDeliveryStatus status) =>
            allGroups.FirstOrDefault(item => item.Channel == channel && item.Status == status)?.Count ?? 0;

        var recentLogs = await _context.ExpiryNotificationLogs
            .Where(log => log.AttemptedAtUtc >= sixMonthStart && log.AttemptedAtUtc <= now)
            .Select(log => new { log.Channel, log.AttemptedAtUtc })
            .ToListAsync(cancellationToken);

        var report = new ExpiryNotificationReportDto
        {
            SmsSucceeded = Count(NotificationChannel.Sms, NotificationDeliveryStatus.Succeeded),
            SmsFailed = Count(NotificationChannel.Sms, NotificationDeliveryStatus.Failed),
            EmailSucceeded = Count(NotificationChannel.Email, NotificationDeliveryStatus.Succeeded),
            EmailFailed = Count(NotificationChannel.Email, NotificationDeliveryStatus.Failed),
            Statuses = allGroups
                .OrderBy(item => item.Channel)
                .ThenBy(item => item.Status)
                .Select(item => new NotificationChannelStatusDto
                {
                    ChannelCode = item.Channel.ToString(),
                    ChannelLabel = item.Channel == NotificationChannel.Sms ? "پیامک" : "ایمیل",
                    StatusCode = item.Status.ToString(),
                    StatusLabel = item.Status == NotificationDeliveryStatus.Succeeded ? "موفق" : "ناموفق",
                    Count = item.Count
                })
                .ToList()
        };

        for (var monthOffset = 5; monthOffset >= 0; monthOffset--)
        {
            var monthStart = currentMonthStart.AddMonths(-monthOffset);
            var nextMonth = monthStart.AddMonths(1);
            report.MonthlyLabels.Add(monthStart.ToString("yyyy/MM"));
            report.MonthlySms.Add(recentLogs.Count(item =>
                item.Channel == NotificationChannel.Sms &&
                item.AttemptedAtUtc >= monthStart && item.AttemptedAtUtc < nextMonth));
            report.MonthlyEmail.Add(recentLogs.Count(item =>
                item.Channel == NotificationChannel.Email &&
                item.AttemptedAtUtc >= monthStart && item.AttemptedAtUtc < nextMonth));
        }

        return report;
    }

    private static string GetDurationLabel(PlanDurationType durationType, int durationInDays)
    {
        return durationType switch
        {
            PlanDurationType.Monthly => "ماهانه",
            PlanDurationType.ThreeMonths => "سه‌ماهه",
            PlanDurationType.SixMonths => "شش‌ماهه",
            PlanDurationType.Annual => "سالانه",
            _ => $"{durationInDays} روزه"
        };
    }
}
