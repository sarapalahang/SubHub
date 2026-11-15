using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileLoginOtp.Application.Interfaces;
using MobileLoginOtp.Domain.Entities;

namespace MobileLoginOtp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly IUserRoleService _userRoleService;

        public RoleController(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        // نمایش لیست نقش‌ها
        public async Task<IActionResult> Index()
        {
            var roles = await _userRoleService.GetAllRolesAsync();
            return View(roles);
        }

        // ایجاد نقش جدید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                await _userRoleService.CreateRoleIfNotExistsAsync(name);
            }
            return RedirectToAction(nameof(Index));
        }

        // نمایش فرم ویرایش (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _userRoleService.GetRoleByIdAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        // ثبت تغییرات ویرایش (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Role model)
        {
            if (ModelState.IsValid)
            {
                await _userRoleService.UpdateRoleAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // حذف نقش
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var isSuccess = await _userRoleService.DeleteRoleAsync(id);
            if (!isSuccess)
            {
                return Json(new { success = false, message = "نقش یافت نشد!" });
            }

            return Json(new { success = true });
        }

        // ۱. نمایش لیست کاربران و نقش‌های فعلی آن‌ها
        [HttpGet]
        public async Task<IActionResult> UserManagement()
        {
            var users = await _userRoleService.GetUsersWithRolesAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int userId, int roleId)
        {
            await _userRoleService.ChangeUserRoleAsync(userId, roleId);
            return RedirectToAction(nameof(UserManagement));
        }
    }
}
