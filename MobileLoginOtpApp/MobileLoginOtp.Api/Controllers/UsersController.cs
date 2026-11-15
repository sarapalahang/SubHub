using Microsoft.AspNetCore.Mvc;
using MobileLoginOtp.Application.Interfaces;

namespace MobileLoginOtp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // آدرس نهایی: api/users/by-phone/09123456789
        [HttpGet("by-phone/{phone}")]
        public async Task<IActionResult> GetUserByPhone(string phone)
        {
            var user = await _userService.GetUserByPhoneNumberAsync(phone);

            if (user == null)
            {
                return NotFound(new { Message = "کاربر پیدا نشد." });
            }

            // برگرداندن اطلاعات کاربر به صورت JSON
            return Ok(new
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            });
        }
    }
}

