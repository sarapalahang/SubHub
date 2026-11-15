using MobileLoginOtp.Domain.Interfaces;

namespace MobileLoginOtp.Domain.Interfaces
{
    public interface ISmsService
    {
        Task<bool> SendOtpSmsAsync(string mobile, string code);
    }
}


