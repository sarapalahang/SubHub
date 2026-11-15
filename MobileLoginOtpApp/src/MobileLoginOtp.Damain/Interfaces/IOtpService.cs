namespace MobileLoginOtp.Domain.Interfaces
{
    public interface IOtpService
    {
        Task<bool> GenerateAndSendOtpAsync(string phoneNumber);
        Task<bool> SendOtpAsync(string phone, string otpCode);
        Task<bool> VerifyOtpAsync(string phoneNumber, string code);
    }
}


