using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileLoginOtp.Domain.Entities;
using System.Reflection.Emit;

namespace MobileLoginOtp.Models.Data.Configrationens
{
    public class OtpRequestConfigration : IEntityTypeConfiguration<OtpRequest>
    {
        public void Configure(EntityTypeBuilder<OtpRequest> builder)
        {
            // تنظیم کلید اصلی
            builder.HasKey(x => x.Id);

            // تنظیم شماره موبایل
            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(11)
                .IsFixedLength(false);

            // تنظیم هش کد (برای امنیت، کد خام را ذخیره نکنید)
            builder.Property(x => x.CodeHash)
                .IsRequired()
                .HasMaxLength(256); // طول مناسب برای انواع الگوریتم‌های هش مثل SHA256

            // فیلدهای وضعیت
            builder.Property(x => x.IsUsed)
                .HasDefaultValue(false);

            builder.Property(x => x.TryCount)
                .HasDefaultValue(0);

            // فیلدهای زمانی
            builder.Property(x => x.CreatedAtUtc)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.ExpiresAtUtc)
                .IsRequired();

            // فیلدهای اختیاری برای امنیت بیشتر و شناسایی کاربر
            builder.Property(x => x.ClientIp)
                .HasMaxLength(50);

            builder.Property(x => x.UserAgent)
                .HasMaxLength(500);

            // ایجاد ایندکس ترکیبی برای جستجوی سریع‌تر کدهای معتبر یک شماره
            builder.HasIndex(x => new { x.PhoneNumber, x.IsUsed, x.ExpiresAtUtc });

            // اگر رابطه با جدول User داری:
            // builder.HasOne(x => x.User)
            //     .WithMany(u => u.OtpRequests)
            //     .HasForeignKey(x => x.UserId)
            //     .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
