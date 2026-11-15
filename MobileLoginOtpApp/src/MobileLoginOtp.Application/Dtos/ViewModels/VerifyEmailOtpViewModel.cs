using System.ComponentModel.DataAnnotations;

namespace MobileLoginOtp.Application.Dtos.ViewModels
{
    public class VerifyEmailOtpViewModel
    {
        public string Email { get; set; }
        [Required(ErrorMessage = "کد تایید الزامی است")]
        public string OtpCode { get; set; }
    }
}
