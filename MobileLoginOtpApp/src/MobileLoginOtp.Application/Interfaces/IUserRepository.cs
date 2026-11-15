using System.Collections.Generic;
using System.Threading.Tasks;
using MobileLoginOtp.Domain.Entities;

namespace MobileLoginOtp.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(long id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<User?> GetByResetPasswordTokenAsync(string token);
        Task<List<User>> UsersWithRolesAsync();
        Task AddAsync(User user);
        void Update(User user);
        Task SaveChangesAsync();
        Task<bool> IsAdminAsync(long userId);
        

    }
}
