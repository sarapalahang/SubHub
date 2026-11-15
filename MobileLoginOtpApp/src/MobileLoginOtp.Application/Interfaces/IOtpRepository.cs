

using MobileLoginOtp.Domain.Entities;

namespace MobileLoginOtp.Application.Interfaces
{
    public interface IOtpRepository
    {
        Task AddOtpRequestAsync(OtpRequest otpRequest);
        Task<OtpRequest?> GetLatestUnusedOtpAsync(string phoneNumber);
        Task SaveChangesAsync();
    }
}
