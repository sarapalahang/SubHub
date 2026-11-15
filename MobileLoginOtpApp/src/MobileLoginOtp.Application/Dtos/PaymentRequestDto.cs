using System;
using System.Collections.Generic;
using System.Text;

namespace MobileLoginOtp.Application.Dtos
{
    public class PaymentRequestDto
    {
        public int UserId { get; set; }
        public long Amount { get; set; }
    }
}
