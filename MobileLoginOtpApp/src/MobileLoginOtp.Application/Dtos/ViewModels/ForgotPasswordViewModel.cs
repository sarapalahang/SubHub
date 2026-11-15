using System.ComponentModel.DataAnnotations;

namespace MobileLoginOtp.Application.Dtos.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        public string Email { get; set; }
    }
}
