using Microsoft.AspNetCore.Http;

namespace MobileLoginOtp.Application.Dtos.ViewModels
{
    public class ProfileViewModel
    {
        public string PhoneNumber { get; set; } = "";
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? ImagePath { get; set; }


        // وضعیت‌ها برای نمایش در صفحه
        public bool PhoneConfirmed { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool HasPassword { get; set; }
        public DateTime? LastLoginAtUtc { get; set; }

        // برای آپلود فایل
        public IFormFile? ProfilePicture { get; set; }

        // برای نمایش عکس فعلی
        public string? ExistingPicturePath { get; set; }
        public DateTime? BirthDate { get; set; }

    }

}
