using Microsoft.EntityFrameworkCore;
using SubHub.Domain.Entities;

namespace SubHub.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Plan> Plans { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<User> Users { get; }
    DbSet<SiteSetting> SiteSettings { get; }
    DbSet<ExpiryNotificationLog> ExpiryNotificationLogs { get; }


    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
