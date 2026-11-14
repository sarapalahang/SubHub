using SubHub.Domain.Enums;

namespace SubHub.Domain.Entities;

public class Invoice
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public long PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    // --- اطلاعات اسنپ‌شات خریدار (جدید) ---
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerPhoneNumber { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }

    // --- اطلاعات مالی و درگاه ---
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Authority { get; set; }
    public string? RefId { get; set; }
    public string PaymentGateway { get; set; } = "ZarinPal";
    public InvoiceStatus Status { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public long? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
