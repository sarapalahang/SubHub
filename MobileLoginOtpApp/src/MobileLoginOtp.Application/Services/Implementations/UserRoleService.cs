using MobileLoginOtp.Application.Dtos.ViewModels;
using MobileLoginOtp.Application.Interfaces;
using MobileLoginOtp.Domain.Entities;
using MobileLoginOtp.Domain.Interfaces;

namespace MobileLoginOtp.Application.Services.Implementations
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public UserRoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllRolesAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _roleRepository.GetRoleByIdAsync(id);
        }

        public async Task CreateRoleIfNotExistsAsync(string name)
        {
            if (!await _roleRepository.RoleExistsAsync(name))
            {
                await _roleRepository.AddRoleAsync(new Role { Name = name });
                await _roleRepository.SaveChangesAsync();
            }
        }

        public async Task UpdateRoleAsync(Role model)
        {
            _roleRepository.UpdateRole(model);
            await _roleRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _roleRepository.GetRoleByIdAsync(id);
            if (role == null) return false;

            _roleRepository.RemoveRole(role);
            await _roleRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserRoleViewModel>> GetUsersWithRolesAsync()
        {
            var users = await _roleRepository.UsersWithRolesAndPaymentsAsync();

            return users.Select(u => new UserRoleViewModel
            {
                UserId = (int)u.Id,
                FullName = (u.FirstName + " " + u.LastName).Trim(),
                PhoneNumber = u.PhoneNumber,
                CurrentRole = u.UserRoles.FirstOrDefault()?.Role?.Name ?? "بدون نقش",
                HasPaidMembership = u.PaymentLogs.Any(p => p.IsPaid),
                LastPaidAt = u.PaymentLogs
                    .Where(p => p.IsPaid)
                    .OrderByDescending(p => p.PaidAt)
                    .Select(p => p.PaidAt)
                    .FirstOrDefault(),
                RefId = u.PaymentLogs
                    .Where(p => p.IsPaid)
                    .OrderByDescending(p => p.PaidAt)
                    .Select(p => p.RefId)
                    .FirstOrDefault(),
                PaidAmount = u.PaymentLogs
                    .Where(p => p.IsPaid)
                    .OrderByDescending(p => p.PaidAt)
                    .Select(p => p.Amount)
                    .FirstOrDefault()
            }).ToList();
        }

        public async Task ChangeUserRoleAsync(int userId, int roleId)
        {
            var currentRoles = await _roleRepository.GetUserRolesByUserIdAsync(userId);

            _roleRepository.RemoveUserRoles(currentRoles);

            await _roleRepository.AddUserRoleAsync(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });

            await _roleRepository.SaveChangesAsync();
        }
    }
}
