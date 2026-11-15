using System.ComponentModel.DataAnnotations;

namespace MobileLoginOtp.Application.Dtos.ViewModels
{
    public class EmailLoginViewModel
    {
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        public string Email { get; set; }

        [Required(ErrorMessage = "پسورد الزامی است")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
