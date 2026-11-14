using System.ComponentModel.DataAnnotations;

namespace SubHub.Admin.Models;

public class AdminLoginViewModel
{
    [Required(ErrorMessage = "نام کاربری را وارد کنید.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "گذرواژه را وارد کنید.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
