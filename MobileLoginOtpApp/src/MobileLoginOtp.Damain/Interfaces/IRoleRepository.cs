using MobileLoginOtp.Domain.Entities;

namespace MobileLoginOtp.Domain.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<bool> RoleExistsAsync(string name);
        Task AddRoleAsync(Role role);
        Task<Role?> GetRoleByIdAsync(int id);
        void UpdateRole(Role role);
        void RemoveRole(Role role);

        Task<List<User>> UsersWithRolesAndPaymentsAsync();
        Task<List<UserRole>> GetUserRolesByUserIdAsync(int userId);

        void RemoveUserRoles(IEnumerable<UserRole> userRoles);
        Task AddUserRoleAsync(UserRole userRole);
        Task SaveChangesAsync();
    }
}
