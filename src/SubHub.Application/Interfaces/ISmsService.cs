namespace SubHub.Application.Interfaces;

public interface ISmsService
{
    Task SendInvoiceLinkSmsAsync(
        string receptor,
        string invoiceNumber,
        string invoiceLink);

    Task SendSmsAsync(
        string receptor,
        string message);
}
