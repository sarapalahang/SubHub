using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MobileLoginOtp.Application.Interfaces;
using MobileLoginOtp.Domain.Entities;
using MobileLoginOtp.Infrastructure.Data;

namespace MobileLoginOtp.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(long id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<User?> GetByResetPasswordTokenAsync(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == token);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsAdminAsync(long userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == "Admin");
        }

        public async Task<List<User>> UsersWithRolesAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }

    }
}
