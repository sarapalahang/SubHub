using Microsoft.Extensions.Options;
using MobileLoginOtp.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using MobileLoginOtp.Domain.Entities;

namespace MobileLoginOtp.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SmsLog> SmsLogs { get; set; }
        public DbSet<OtpRequest> OtpRequests { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<PaymentLog> PaymentLogs { get; set; }

    }
}







