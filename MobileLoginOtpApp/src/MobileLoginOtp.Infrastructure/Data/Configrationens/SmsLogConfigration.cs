using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileLoginOtp.Domain.Entities;


namespace MobileLoginOtp.Models.Data.Configrationens
{
    public class SmsLogConfigration : IEntityTypeConfiguration<SmsLog>
    
    {
        public void Configure(EntityTypeBuilder<SmsLog> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(11)
                .IsFixedLength(false);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.ProviderMessageId)
                .HasMaxLength(100);

            builder.Property(x => x.IsSuccess)
                .HasDefaultValue(false);

            builder.Property(x => x.Error)
                .HasMaxLength(1000);

            builder.Property(x => x.CreatedAtUtc)
                .HasDefaultValueSql("GETUTCDATE()");

            // ایندکس برای جستجوی سریع لاگ‌های یک شماره
            builder.HasIndex(x => x.PhoneNumber);

            // ایندکس برای بررسی وضعیت ارسال
            builder.HasIndex(x => new { x.PhoneNumber, x.IsSuccess });
        }
    }
}
