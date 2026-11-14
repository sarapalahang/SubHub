namespace SubHub.Contracts.Invoices;

public class PaymentCallbackResult
{
    public bool IsSuccess { get; set; }
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? RefId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserPhoneNumber { get; set; } // این خط اضافه شد
}
