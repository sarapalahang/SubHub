using MobileLoginOtp.Application.Interfaces;
using MobileLoginOtp.Domain.Entities;
using MobileLoginOtp.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace MobileLoginOtp.Application.Services.Implementations
{
    public class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepository; // دیتابکس حذف و ریپازیتوری جایگزین شد
        private readonly ISmsService _smsService;

        public OtpService(IOtpRepository otpRepository, ISmsService smsService)
        {
            _otpRepository = otpRepository;
            _smsService = smsService;
        }

        public async Task<bool> GenerateAndSendOtpAsync(string phoneNumber)
        {
            // تولید کد ۶ رقمی امن
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var hash = HashCode(code);

            var otp = new OtpRequest
            {
                PhoneNumber = phoneNumber,
                CodeHash = hash,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(2),
                IsUsed = false,
                TryCount = 0
            };

            await _otpRepository.AddOtpRequestAsync(otp);
            await _otpRepository.SaveChangesAsync();

            await _smsService.SendOtpSmsAsync(phoneNumber, code);

            return true;
        }

        public async Task<bool> SendOtpAsync(string phone, string otpCode)
        {
            return await _smsService.SendOtpSmsAsync(phone, otpCode);
        }

        public async Task<bool> VerifyOtpAsync(string phoneNumber, string code)
        {
            var otp = await _otpRepository.GetLatestUnusedOtpAsync(phoneNumber);

            if (otp == null)
                return false;

            if (otp.ExpiresAtUtc < DateTime.UtcNow)
                return false;

            var hash = HashCode(code);

            if (otp.CodeHash != hash)
            {
                otp.TryCount++;
                await _otpRepository.SaveChangesAsync();
                return false;
            }

            otp.IsUsed = true;
            await _otpRepository.SaveChangesAsync();

            return true;
        }

        private string HashCode(string code)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(code);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
