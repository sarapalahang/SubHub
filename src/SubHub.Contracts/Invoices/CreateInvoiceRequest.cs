namespace SubHub.Contracts.Invoices;

public class CreateInvoiceRequest
{
    public Guid UserId { get; set; }
    public long PlanId { get; set; }

    public string BuyerName { get; set; } = string.Empty;
    public string BuyerPhoneNumber { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
}
