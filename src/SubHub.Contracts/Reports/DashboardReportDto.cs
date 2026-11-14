namespace SubHub.Contracts.Reports;

public class DashboardReportDto
{
    // آمارهای کلی (کارت‌های بالای داشبورد)
    public decimal TotalRevenue { get; set; }           // کل درآمد حاصل از فاکتورهای پرداخت‌شده
    public int TotalSubscribers { get; set; }           // تعداد کل کاربران / مشترکین
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }        // تعداد اشتراک‌های فعال
    public int CancelledSubscriptionsCount { get; set; }
    public decimal CancellationRatePercentage { get; set; }
    public int PendingInvoicesCount { get; set; }       // فاکتورهای در انتظار پرداخت

    // داده‌های نمودار فروش ۶ ماه گذشته
    public List<string> MonthlyLabels { get; set; } = new();
    public List<decimal> MonthlyRevenues { get; set; } = new();
    public List<int> MonthlyCancelledSubscriptions { get; set; } = new();
    public List<CancellationReasonReportDto> CancellationReasons { get; set; } = new();
    public List<RecentCancellationDto> RecentCancellations { get; set; } = new();

    // لیست آخرین فاکتورها برای جدول داشبورد
    public List<RecentInvoiceDto> RecentInvoices { get; set; } = new();
}

public class CancellationReasonReportDto
{
    public string Reason { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentCancellationDto
{
    public string PlanTitle { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
}

public class RecentInvoiceDto
{
    public Guid Id { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string PlanTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
