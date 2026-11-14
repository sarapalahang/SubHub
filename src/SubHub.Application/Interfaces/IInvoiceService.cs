using SubHub.Contracts.Invoices;

namespace SubHub.Application.Interfaces;

public interface IInvoiceService
{
    // ۱. ایجاد پیش‌فاکتور و دریافت آدرس انتقال به درگاه
    Task<CreateInvoiceResponse> CreateInvoiceAndInitiatePaymentAsync(
        CreateInvoiceRequest request,
        string callbackUrl,
        string? userEmail = null,
        string? userMobile = null,
        CancellationToken cancellationToken = default);


    // ۲. تأیید تراکنش پس از بازگشت کاربر از درگاه و فعال‌سازی اشتراک
    Task<PaymentCallbackResult> VerifyAndProcessPaymentAsync(
        string authority,
        string status,
        CancellationToken cancellationToken = default);

    Task<int> ReconcilePendingInvoicesAsync(
        CancellationToken cancellationToken = default);

}
