using System.ComponentModel.DataAnnotations;
namespace MobileLoginOtp.Domain.Entities;

    public class OtpRequest
    {
        public long Id { get; set; }

        public string PhoneNumber { get; set; } = null!;
        public string CodeHash { get; set; } = null!;      // Hash(code + salt)

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAtUtc { get; set; }          // مثلا +2 دقیقه

        public bool IsUsed { get; set; } = false;
        public int TryCount { get; set; } = 0;
        public DateTime? UsedAtUtc { get; set; }

        public string? ClientIp { get; set; }              // برای امنیت/لاگ
        public string? UserAgent { get; set; }

    }




