namespace MobileLoginOtp.Application.Dtos
{
    public class OtpResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RedirectUrl { get; set; }
    }
}
