using Microsoft.EntityFrameworkCore;
using MobileLoginOtp.Application.Interfaces;
using MobileLoginOtp.Application.Services;
using MobileLoginOtp.Application.Services.Implementations;
using MobileLoginOtp.Domain.Interfaces;
using MobileLoginOtp.Infrastructure.Data;
using MobileLoginOtp.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ۱. تنظیم دیتابیس


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MobileLoginOtp")));

// ۲. ثبت سرویس‌ها
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IOtpService, OtpService>();



//// ۳. تنظیمات CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
//    });
//});
// ۱. تعریف CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // بدون پارامتر اضافه
builder.Services.AddAuthorization();

var app = builder.Build();

// ۵. فعال‌سازی سواگر در محیط توسعه
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // بدون پارامتر اضافه
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers(); // فقط اگر API Controller هم داری

app.Run();
