using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MobileLoginOtp.Domain.Entities;

namespace MobileLoginOtp.Infrastructure.Data.Configurations
{
    public class PaymentLogConfiguration : IEntityTypeConfiguration<PaymentLog>
    {
        public void Configure(EntityTypeBuilder<PaymentLog> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Authority)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.RefId)
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

