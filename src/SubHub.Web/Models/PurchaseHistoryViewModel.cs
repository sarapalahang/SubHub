namespace SubHub.Web.Models;

public class PurchaseHistoryItemViewModel
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal Amount { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = "bg-secondary";
    public string CreatedAtPersian { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

}

public class PurchaseHistoryViewModel
{
    public List<PurchaseHistoryItemViewModel> Items { get; set; } = new();
}
