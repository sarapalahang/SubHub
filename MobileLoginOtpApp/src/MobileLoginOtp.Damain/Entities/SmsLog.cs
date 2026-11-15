using System.ComponentModel.DataAnnotations;
namespace MobileLoginOtp.Domain.Entities
{
    public class SmsLog
    {
        public long Id { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? ProviderMessageId { get; set; }

        public bool IsSuccess { get; set; }
        public string? Error { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

}
