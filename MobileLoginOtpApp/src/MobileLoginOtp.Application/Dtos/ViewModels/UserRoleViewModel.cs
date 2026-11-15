using MobileLoginOtp.Domain.Entities;

namespace MobileLoginOtp.Application.Dtos.ViewModels
{
    public class UserRoleViewModel
    {
        public long UserId { get; set; } // نوع داده با Entity همخوانی دارد
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public string? ImagePath { get; set; }

        // اطلاعات پرداخت
        public bool HasPaidMembership { get; set; }
        public DateTime? LastPaidAt { get; set; }
        public string? RefId { get; set; }
        public decimal? PaidAmount { get; set; } // Nullable برای حالتی که پرداخت ندارد

        public List<Role> AllRoles { get; set; } = new List<Role>();
    }
}
