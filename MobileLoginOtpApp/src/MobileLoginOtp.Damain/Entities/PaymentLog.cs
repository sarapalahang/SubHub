using System;
using System.Collections.Generic;
using System.Text;

namespace MobileLoginOtp.Domain.Entities
{
    public class PaymentLog
    {
        public int Id { get; set; }

        public long UserId { get; set; }

        public long Amount { get; set; }

        public string Authority { get; set; } = null!;

        public string? RefId { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }

        public string? Description { get; set; }

        public User? User { get; set; }
    }
}
