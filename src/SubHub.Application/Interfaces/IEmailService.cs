namespace SubHub.Application.Interfaces;

public interface IEmailService
{
    //Task SendInvoiceEmailAsync(string toEmail, string invoiceNumber, string invoiceUrl);
    // متد جدید برای ارسال ایمیل با فایل PDF ضمیمه
    Task SendInvoiceEmailWithPdfAsync(string toEmail, string invoiceNumber, byte[]? pdfBytes);
    //Task SendInvoiceEmailWithPdfAsync(string toEmail, string invoiceNumber, byte[] pdfBytes); // یا هر امضایی که استفاده می‌کنید

    Task SendSubscriptionExpiryEmailAsync(string toEmail, string planName, int daysRemaining);

}
