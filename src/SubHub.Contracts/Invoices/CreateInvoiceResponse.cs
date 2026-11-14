namespace SubHub.Contracts.Invoices;

public class CreateInvoiceResponse
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
}
