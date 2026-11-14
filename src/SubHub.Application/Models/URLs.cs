using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SubHub.Application.Models
{
    public class URLs
    {
        // آدرس‌های تست (Sandbox)
        public const String gateWayUrl = "https://sandbox.zarinpal.com/pg/StartPay/";
        public const String requestUrl = "https://sandbox.zarinpal.com/pg/v4/payment/request.json";
        public const String verifyUrl = "https://sandbox.zarinpal.com/pg/v4/payment/verify.json";
        public const string inquiryUrl = "https://sandbox.zarinpal.com/pg/v4/payment/inquiry.json";
    }
}

