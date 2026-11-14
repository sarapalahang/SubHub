namespace SubHub.Domain.Enums;

public enum SubscriptionCancellationReason
{
    Price = 1,
    MissingFeatures = 2,
    TechnicalIssues = 3,
    NoLongerNeeded = 4,
    Other = 5
}

public static class SubscriptionCancellationReasonExtensions
{
    public static string ToPersianLabel(this SubscriptionCancellationReason reason) => reason switch
    {
        SubscriptionCancellationReason.Price => "هزینه اشتراک بالا بود",
        SubscriptionCancellationReason.MissingFeatures => "امکانات موردنیازم را نداشت",
        SubscriptionCancellationReason.TechnicalIssues => "مشکل فنی داشتم",
        SubscriptionCancellationReason.NoLongerNeeded => "دیگر به سرویس نیاز ندارم",
        SubscriptionCancellationReason.Other => "دلیل دیگر",
        _ => "نامشخص"
    };
}
