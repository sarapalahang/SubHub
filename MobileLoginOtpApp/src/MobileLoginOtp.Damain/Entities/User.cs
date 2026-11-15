using System.ComponentModel.DataAnnotations;

namespace MobileLoginOtp.Domain.Entities
{
    public class User
    {
        public long Id { get; set; }

        [Required, MaxLength(15)]
        public string? PhoneNumber { get; set; } 

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [EmailAddress, MaxLength(200)]
        public string? Email { get; set; }

        // وضعیت‌ها
        public bool PhoneConfirmed { get; set; }
        public bool EmailConfirmed { get; set; }

        // OTP ایمیل
        public string? EmailOtpCode { get; set; }
        public DateTime? EmailOtpExpiry { get; set; }

        // OTP پیامک
        public string? SmsOtpCode { get; set; }
        public DateTime? SmsOtpExpiry { get; set; }

        // فراموشی رمز عبور
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }

        //public bool IsActive { get; set; } = true;
        public bool IsActive { get; set; } = false;
        public DateTime? ActivatedAt { get; set; }


        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAtUtc { get; set; }

        // پسورد (اختیاری)
        public string? PasswordHash { get; set; }

        // رابطه با جدول نقش‌ها
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<PaymentLog> PaymentLogs { get; set; } = new List<PaymentLog>();
        public string? ImagePath { get; set; }

        public DateTime? BirthDate { get; set; }


    }
}




