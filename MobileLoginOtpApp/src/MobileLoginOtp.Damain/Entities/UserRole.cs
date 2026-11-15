using System.ComponentModel.DataAnnotations;// این خط خیلی مهم است

namespace MobileLoginOtp.Domain.Entities
{
    public class UserRole
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        // اینجا باید مدل User شناخته شده باشد
        public User User { get; set; }

        public int RoleId { get; set; }

        // اینجا هم باید مدل Role شناخته شده باشد
        public Role Role { get; set; }
    }
}
