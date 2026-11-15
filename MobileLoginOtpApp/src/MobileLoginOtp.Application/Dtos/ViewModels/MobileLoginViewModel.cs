using System.ComponentModel.DataAnnotations;

namespace MobileLoginOtp.Application.Dtos.ViewModels
{
    public class MobileLoginViewModel
    {
        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [Display(Name = "شماره موبایل")]
        public string MobileNumber { get; set; }
    }
}
