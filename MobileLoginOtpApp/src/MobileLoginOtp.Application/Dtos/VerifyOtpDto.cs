namespace MobileLoginOtp.Application.Dtos
{
    public class VerifyOtpDto
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Code { get; set; }
    }
}
