using System;
using System.Collections.Generic;
using System.Text;

namespace MobileLoginOtp.Application.Dtos.ViewModels
{
    public class UserRoleManagementViewModel
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }

        public string CurrentRole { get; set; } = "بدون نقش";

        // عضویت/پرداخت
        public bool HasPaidMembership { get; set; }
        public DateTime? LastPaidAt { get; set; }
        public string? RefId { get; set; }
        public long? PaidAmount { get; set; }
    }
}

