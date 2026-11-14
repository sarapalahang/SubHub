using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SubHub.Application.Interfaces;
using SubHub.Domain.Entities;
using System.Globalization;

namespace SubHub.Application.Services;

public class PdfService : IPdfService
{
    public PdfService()
    {
        // تنظیم لایسنس رایگان QuestPDF
        QuestPDF.Settings.License = LicenseType.Community;

        // فعال‌سازی فونت‌های سیستم یا غیرفعال کردن بررسی سخت‌گیرانه فونت
        QuestPDF.Settings.UseSystemFonts = true;
        QuestPDF.Settings.ThrowOnMissingTextGlyphs = false;
    }


    public byte[] GenerateInvoicePdf(Invoice invoice)
    {
        var persianCalendar = new PersianCalendar();
        var date = invoice.PaidAtUtc ?? invoice.CreatedAtUtc;
        var persianDate = $"{persianCalendar.GetYear(date)}/{persianCalendar.GetMonth(date):00}/{persianCalendar.GetDayOfMonth(date):00}";

        var buyerName = string.IsNullOrWhiteSpace(invoice.BuyerName) ? "مشتری گرامی" : invoice.BuyerName;
        var buyerPhone = string.IsNullOrWhiteSpace(invoice.BuyerPhoneNumber) ? "-" : invoice.BuyerPhoneNumber;
        var buyerEmail = string.IsNullOrWhiteSpace(invoice.BuyerEmail) ? "-" : invoice.BuyerEmail;
        var planTitle = invoice.Plan?.Title ?? invoice.Plan?.Name ?? "اشتراک SubHub";
        var trackingCode = string.IsNullOrWhiteSpace(invoice.RefId) ? "-" : invoice.RefId;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.Margin(20);
                page.PageColor(Colors.White);

                // فونت Arial دارای گلیف‌های استاندارد است و Skia کرش نمی‌کند
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                page.Content().Column(col =>
                {
                    col.Spacing(7);

                    col.Item().Text("SubHub Invoice / رسید خرید اشتراک").FontSize(15).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Text($"شماره فاکتور: {invoice.InvoiceNumber}");
                    col.Item().Text($"تاریخ پرداخت: {persianDate}");
                    col.Item().Text($"کد رهگیری: {trackingCode}");
                    col.Item().Text("وضعیت پرداخت: موفق").FontColor(Colors.Green.Darken2).Bold();

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Text($"نام خریدار: {buyerName}");
                    col.Item().Text($"شماره تماس: {buyerPhone}");
                    col.Item().Text($"ایمیل خریدار: {buyerEmail}");

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Text($"عنوان پلن: {planTitle}").Bold();
                    col.Item().Text($"مبلغ کل: {invoice.Amount:N0} تومان").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);

                    col.Item().PaddingTop(10);
                    col.Item().Text("با تشکر از خرید شما - سامانه ساب‌هاب").FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
    }
}
