namespace SubHub.Domain.Enums;

public enum InvoiceStatus
{
    PendingPayment = 1, // در انتظار پرداخت (پیش‌فاکتور)
    Paid = 2,           // پرداخت شده (فاکتور قطعی)
    Cancelled = 3,      // لغو شده توسط کاربر یا سیستم
    Expired = 4,         // منقضی شده
    Pending = 5,
    Failed = 6
}
