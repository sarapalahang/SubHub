namespace SubHub.Web.Models; 

public class CurrentSubscriptionViewModel
{
    public long SubscriptionId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string? PlanTitle { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public SubHub.Domain.Enums.SubscriptionStatus Status { get; set; }
    public bool IsActiveNow { get; set; }
    public bool CanCancel { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public SubHub.Domain.Enums.SubscriptionCancellationReason? CancellationReason { get; set; }
    public string? CancellationReasonDetails { get; set; }
    public string? CancellationReasonLabel => CancellationReason.HasValue
        ? SubHub.Domain.Enums.SubscriptionCancellationReasonExtensions.ToPersianLabel(CancellationReason.Value)
        : null;
    public decimal? Amount { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}
