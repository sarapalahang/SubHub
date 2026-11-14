using SubHub.Domain.Common;
using SubHub.Domain.Enums;

namespace SubHub.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }

    public long PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public DateTime? LastExpirySmsNotificationDateUtc { get; set; }

    public DateTime? LastExpiryEmailNotificationDateUtc { get; set; }

    public DateTime? CancelledAtUtc { get; set; }

    public SubscriptionCancellationReason? CancellationReason { get; set; }

    public string? CancellationReasonDetails { get; set; }

    public bool IsActiveNow => Status == SubscriptionStatus.Active && EndDateUtc >= DateTime.UtcNow;
}
