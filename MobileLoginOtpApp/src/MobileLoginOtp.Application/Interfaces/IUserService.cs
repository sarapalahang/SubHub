

using MobileLoginOtp.Domain.Entities;

namespace MobileLoginOtp.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> GetOrCreateByEmailAsync(string email);
        Task<User> GetOrCreateByPhoneNumberAsync(string phoneNumber);
        Task<User> GetByIdAsync(long id);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByPhoneNumberAsync(string phoneNumber);
        Task UpdateAsync(User user);
        Task SaveChangesAsync();
        Task<bool> IsAdminAsync(long userId);
        Task<User?> GetUserByPhoneNumberAsync(string phoneNumber);

    }

}
