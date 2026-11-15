using MobileLoginOtp.Application.Interfaces;
using MobileLoginOtp.Domain.Entities;
using MobileLoginOtp.Domain.Interfaces;

namespace MobileLoginOtp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetOrCreateByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email.Trim().ToLower());
            if (user != null) return user;

            user = new User { Email = email.Trim().ToLower() };
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetOrCreateByPhoneNumberAsync(string phoneNumber)
        {
            var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber.Trim());
            if (user != null) return user;

            user = new User { PhoneNumber = phoneNumber.Trim() };
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            return user;
        }


        public Task<User?> GetByIdAsync(long id) => _userRepository.GetByIdAsync(id);

        public Task<User?> GetByEmailAsync(string email) => _userRepository.GetByEmailAsync(email.Trim().ToLower());

        public Task<User?> GetByPhoneNumberAsync(string phoneNumber) => _userRepository.GetByPhoneNumberAsync(phoneNumber.Trim());

        public async Task UpdateAsync(User user)
        {
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _userRepository.SaveChangesAsync();

        // پیاده‌سازی متدی که باعث خطای CS0535 شده بود
        public async Task<bool> IsAdminAsync(long userId)
        {
            return await _userRepository.IsAdminAsync(userId);
        }
        public async Task<User?> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            return await _userRepository.GetByPhoneNumberAsync(phoneNumber);
        }

    }
}
