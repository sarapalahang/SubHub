using SubHub.Domain.Common;
using SubHub.Domain.Enums;

namespace SubHub.Domain.Entities;

public class ExpiryNotificationLog : BaseEntity
{
    public long SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public NotificationChannel Channel { get; set; }
    public NotificationDeliveryStatus Status { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int DaysRemaining { get; set; }
    public DateTime AttemptedAtUtc { get; set; } = DateTime.UtcNow;
}
