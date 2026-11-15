using System.ComponentModel.DataAnnotations;

namespace MobileLoginOtp.Application.Dtos.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "شماره موبایل")]
        // این فیلد را از قبل داشتی
        public string? PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        public string? Email { get; set; }

        [Display(Name = "رمز عبور")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
