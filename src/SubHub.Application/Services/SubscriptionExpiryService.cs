using Microsoft.EntityFrameworkCore;
using SubHub.Application.Interfaces;
using SubHub.Domain.Entities;
using SubHub.Domain.Enums;

namespace SubHub.Application.Services;

public class SubscriptionExpiryService : ISubscriptionExpiryService
{
    private static readonly int[] ReminderDays = [3, 2, 1];

    private readonly IApplicationDbContext _context;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;

    public SubscriptionExpiryService(
        IApplicationDbContext context,
        ISmsService smsService,
        IEmailService emailService)
    {
        _context = context;
        _smsService = smsService;
        _emailService = emailService;
    }

    public async Task ProcessSubscriptionExpirationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayUtc = now.Date;
        var settings = await _context.SiteSettings.FirstOrDefaultAsync(cancellationToken)
            ?? new SiteSetting();

        var reminderSubscriptions = await _context.Subscriptions
            .Include(subscription => subscription.Plan)
            .Where(subscription =>
                subscription.Status == SubscriptionStatus.Active &&
                subscription.EndDateUtc >= todayUtc.AddDays(1) &&
                subscription.EndDateUtc < todayUtc.AddDays(4))
            .ToListAsync(cancellationToken);

        var remindersForToday = reminderSubscriptions
            .Select(subscription => new
            {
                Subscription = subscription,
                DaysRemaining = (subscription.EndDateUtc.Date - todayUtc).Days
            })
            .Where(item => ReminderDays.Contains(item.DaysRemaining))
            .ToList();

        var subscriptionIds = remindersForToday
            .Select(item => item.Subscription.Id)
            .ToList();
        var userIds = remindersForToday
            .Select(item => item.Subscription.UserId)
            .Distinct()
            .ToList();

        var userContacts = await _context.Users
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(
                user => user.Id,
                user => new { user.PhoneNumber, user.Email },
                cancellationToken);

        var invoiceContacts = await _context.Invoices
            .Where(invoice =>
                invoice.SubscriptionId.HasValue &&
                subscriptionIds.Contains(invoice.SubscriptionId.Value) &&
                invoice.Status == InvoiceStatus.Paid &&
                invoice.BuyerEmail != null &&
                invoice.BuyerEmail != "")
            .OrderByDescending(invoice => invoice.CreatedAtUtc)
            .Select(invoice => new
            {
                SubscriptionId = invoice.SubscriptionId!.Value,
                invoice.BuyerEmail
            })
            .ToListAsync(cancellationToken);

        var emailAddresses = new Dictionary<long, string>();
        foreach (var contact in invoiceContacts)
            emailAddresses.TryAdd(contact.SubscriptionId, contact.BuyerEmail!);

        var notificationErrors = new List<Exception>();

        foreach (var reminder in remindersForToday)
        {
            var subscription = reminder.Subscription;
            var message = CreateReminderMessage(subscription.Plan.Title, reminder.DaysRemaining);

            if (settings.EnableSmsNotification &&
                userContacts.TryGetValue(subscription.UserId, out var userContact) &&
                !string.IsNullOrWhiteSpace(userContact.PhoneNumber) &&
                subscription.LastExpirySmsNotificationDateUtc?.Date != todayUtc)
            {
                var log = new ExpiryNotificationLog
                {
                    SubscriptionId = subscription.Id,
                    Channel = NotificationChannel.Sms,
                    Recipient = MaskRecipient(userContact.PhoneNumber),
                    DaysRemaining = reminder.DaysRemaining,
                    AttemptedAtUtc = now
                };
                try
                {
                    await _smsService.SendSmsAsync(userContact.PhoneNumber, message);
                    log.Status = NotificationDeliveryStatus.Succeeded;
                    subscription.LastExpirySmsNotificationDateUtc = now;
                }
                catch (Exception exception)
                {
                    log.Status = NotificationDeliveryStatus.Failed;
                    log.ErrorMessage = exception.Message.Length > 2000 ? exception.Message[..2000] : exception.Message;
                    notificationErrors.Add(exception);
                }

                _context.ExpiryNotificationLogs.Add(log);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var emailAddress = userContacts.TryGetValue(subscription.UserId, out var emailContact)
                ? emailContact.Email
                : null;
            if (string.IsNullOrWhiteSpace(emailAddress))
                emailAddresses.TryGetValue(subscription.Id, out emailAddress);

            if (settings.EnableEmailNotification &&
                !string.IsNullOrWhiteSpace(emailAddress) &&
                subscription.LastExpiryEmailNotificationDateUtc?.Date != todayUtc)
            {
                var log = new ExpiryNotificationLog
                {
                    SubscriptionId = subscription.Id,
                    Channel = NotificationChannel.Email,
                    Recipient = MaskRecipient(emailAddress),
                    DaysRemaining = reminder.DaysRemaining,
                    AttemptedAtUtc = now
                };
                try
                {
                    await _emailService.SendSubscriptionExpiryEmailAsync(
                        emailAddress,
                        subscription.Plan.Title,
                        reminder.DaysRemaining);
                    log.Status = NotificationDeliveryStatus.Succeeded;
                    subscription.LastExpiryEmailNotificationDateUtc = now;
                }
                catch (Exception exception)
                {
                    log.Status = NotificationDeliveryStatus.Failed;
                    log.ErrorMessage = exception.Message.Length > 2000 ? exception.Message[..2000] : exception.Message;
                    notificationErrors.Add(exception);
                }

                _context.ExpiryNotificationLogs.Add(log);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var expiredSubscriptions = await _context.Subscriptions
            .Where(subscription =>
                subscription.Status == SubscriptionStatus.Active &&
                subscription.EndDateUtc <= now)
            .ToListAsync(cancellationToken);

        foreach (var subscription in expiredSubscriptions)
            subscription.Status = SubscriptionStatus.Expired;

        if (expiredSubscriptions.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);

        if (notificationErrors.Count > 0)
            throw new AggregateException("One or more subscription expiry notifications failed.", notificationErrors);
    }

    private static string CreateReminderMessage(string planName, int daysRemaining)
    {
        var remainingTime = daysRemaining == 1 ? "فردا" : $"تا {daysRemaining} روز دیگر";
        return $"اشتراک طرح {planName} {remainingTime} به پایان می‌رسد. برای ادامه استفاده، اشتراک خود را تمدید کنید.";
    }

    private static string MaskRecipient(string recipient)
    {
        var atIndex = recipient.IndexOf('@');
        if (atIndex > 0)
            return $"{recipient[0]}***{recipient[atIndex..]}";

        var digits = new string(recipient.Where(char.IsDigit).ToArray());
        return digits.Length > 4 ? $"***{digits[^4..]}" : "****";
    }
}
