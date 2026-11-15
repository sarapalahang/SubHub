using System.ComponentModel.DataAnnotations;

namespace MobileLoginOtp.Application.Dtos.ViewModels
{
    public class SetPasswordViewModel
    {
        [Required, MinLength(6)]
        public string Password { get; set; } = "";

        [Required, Compare(nameof(Password), ErrorMessage = "تکرار پسورد با پسورد یکسان نیست.")]
        public string ConfirmPassword { get; set; } = "";
    }
}

