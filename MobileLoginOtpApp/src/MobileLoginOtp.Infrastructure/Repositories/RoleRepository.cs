using Microsoft.EntityFrameworkCore;
using MobileLoginOtp.Domain.Entities;
using MobileLoginOtp.Domain.Interfaces;
using MobileLoginOtp.Infrastructure.Data;

namespace MobileLoginOtp.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<bool> RoleExistsAsync(string name)
        {
            return await _context.Roles.AnyAsync(r => r.Name == name);
        }

        public async Task AddRoleAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public void UpdateRole(Role role)
        {
            _context.Roles.Update(role);
        }

        public void RemoveRole(Role role)
        {
            _context.Roles.Remove(role);
        }

        public async Task<List<User>> UsersWithRolesAndPaymentsAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.PaymentLogs)
                .ToListAsync();
        }

        public async Task<List<UserRole>> GetUserRolesByUserIdAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
        }

        public void RemoveUserRoles(IEnumerable<UserRole> userRoles)
        {
            _context.UserRoles.RemoveRange(userRoles);
        }

        public async Task AddUserRoleAsync(UserRole userRole)
        {
            await _context.UserRoles.AddAsync(userRole);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
