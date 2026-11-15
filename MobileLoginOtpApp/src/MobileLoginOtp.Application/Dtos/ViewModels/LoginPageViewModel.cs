

namespace MobileLoginOtp.Application.Dtos.ViewModels
{
    public class LoginPageViewModel
    {
        public MobileLoginViewModel Mobile { get; set; } = new();
        public EmailLoginViewModel Email { get; set; } = new();

        // برای اینکه تب فعال بعد از خطا حفظ شود (mobile یا email)
        public string ActiveTab { get; set; } = "mobile";
    }
}

