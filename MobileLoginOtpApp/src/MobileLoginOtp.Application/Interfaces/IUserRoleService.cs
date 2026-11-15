using MobileLoginOtp.Application.Dtos.ViewModels;
using MobileLoginOtp.Domain.Entities;

namespace MobileLoginOtp.Application.Interfaces
{
    public interface IUserRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int id);
        Task CreateRoleIfNotExistsAsync(string name);
        Task UpdateRoleAsync(Role model);
        Task<bool> DeleteRoleAsync(int id);

        // اصلاح نام کلاس خروجی: استفاده از UserRoleViewModel
        Task<List<UserRoleViewModel>> GetUsersWithRolesAsync();

        Task ChangeUserRoleAsync(int userId, int roleId);
    }
}
