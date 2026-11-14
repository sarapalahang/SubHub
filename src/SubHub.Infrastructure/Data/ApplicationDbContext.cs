using Microsoft.EntityFrameworkCore;
using SubHub.Application.Interfaces;
using SubHub.Domain.Common;
using SubHub.Domain.Entities;

namespace SubHub.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SiteSetting> SiteSettings { get; set; }
    public DbSet<ExpiryNotificationLog> ExpiryNotificationLogs => Set<ExpiryNotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // تنظیمات جدول پلن‌ها (Plans)
        modelBuilder.Entity<Plan>(builder =>
        {
            builder.ToTable("Plans");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasMaxLength(500);

            builder.Property(p => p.Price)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("Users");
            builder.HasKey(user => user.Id);

            builder.Property(user => user.Email)
                .HasMaxLength(256);

            builder.Property(user => user.CreatedAtUtc);
        });

        // تنظیمات جدول اشتراک‌ها (Subscriptions)
        modelBuilder.Entity<Subscription>(builder =>
        {
            builder.ToTable("Subscriptions");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.UserId)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(s => s.CancellationReasonDetails)
                .HasMaxLength(500);

            builder.HasOne(s => s.Plan)
                .WithMany()
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // تنظیمات جدول فاکتورها (Invoices)
        modelBuilder.Entity<Invoice>(builder =>
        {
            builder.ToTable("Invoices");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.UserId)
                .IsRequired();

            builder.Property(i => i.PlanId)
                .IsRequired();

            builder.Property(i => i.InvoiceNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            builder.Property(i => i.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(i => i.Status)
                .IsRequired();

            builder.Property(i => i.Authority)
                .HasMaxLength(100);

            builder.Property(i => i.RefId)
                .HasMaxLength(100);

            builder.Property(i => i.PaymentGateway)
                .HasMaxLength(64)
                .IsRequired();

            builder.HasOne(i => i.Plan)
                .WithMany()
                .HasForeignKey(i => i.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Subscription)
                .WithMany()
                .HasForeignKey(i => i.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExpiryNotificationLog>(builder =>
        {
            builder.ToTable("ExpiryNotificationLogs");
            builder.HasKey(log => log.Id);
            builder.Property(log => log.Recipient).HasMaxLength(256).IsRequired();
            builder.Property(log => log.ErrorMessage).HasMaxLength(2000);
            builder.HasIndex(log => log.AttemptedAtUtc);
            builder.HasIndex(log => new { log.Channel, log.Status, log.AttemptedAtUtc });
            builder.HasOne(log => log.Subscription)
                .WithMany()
                .HasForeignKey(log => log.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // به‌روزرسانی خودکار فیلد UpdatedAtUtc برای انتیتی‌های تغییریافته
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
