using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileLoginOtp.Domain.Entities;
using System.Reflection.Emit;

namespace MobileLoginOtp.Infrastructure.Data.Configrationens
{
    public class UserConfigration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // تنظیم کلید اصلی
            builder.HasKey(x => x.Id);

            // تنظیمات شماره موبایل
            builder.Property(x => x.PhoneNumber)
                .IsRequired(false)
                .HasMaxLength(11) // برای شماره‌های ایران 11 رقم
                .IsFixedLength(false); // چون شماره موبایل همیشه ثابت است (اختیاری)

            // ایجاد ایندکس یکتا روی شماره موبایل (بسیار مهم برای لاگین)
            builder.HasIndex(x => x.PhoneNumber)
                .IsUnique()
                .HasFilter("[PhoneNumber] IS NOT NULL"); // تغییر مهم 3: جلوگیری از تداخل مقادیر خالی;


            // تنظیم مقدار پیش‌فرض برای وضعیت فعال بودن
            builder.Property(x => x.IsActive)
                .HasDefaultValue(false);

            // تنظیم تاریخ ایجاد (می‌تواند از سمت دیتابیس هم ست شود)
            builder.Property(x => x.CreatedAtUtc)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.BirthDate)
                .HasColumnType("date"); // فقط تاریخ را ذخیره می‌کند (بدون ساعت)
                                   

            // اگر فیلد دیگری مثل نام یا نقش داری اینجا اضافه کن
        }
    }
}
