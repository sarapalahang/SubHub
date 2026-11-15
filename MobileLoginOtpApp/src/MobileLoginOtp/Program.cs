using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MobileLoginOtp.Application.Interfaces;
using MobileLoginOtp.Application.Services;
using MobileLoginOtp.Application.Services.Implementations;
using MobileLoginOtp.Domain.Entities;
using MobileLoginOtp.Domain.Interfaces;
using MobileLoginOtp.Infrastructure.Data;
using MobileLoginOtp.Infrastructure.Repositories;
using AppDbContext = MobileLoginOtp.Infrastructure.Data.AppDbContext;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// Register Services
// ---------------------------

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();

builder.Services.AddHttpClient();
builder.Services.Configure<SmsSettings>(
    builder.Configuration.GetSection("SmsSettings"));
builder.Services.AddHttpClient<ISmsService, SmsService>();

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MobileLoginOtp"),
        sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

builder.Services.Configure<GapGptSettings>(
    builder.Configuration.GetSection("GapGpt"));

builder.Services.AddHttpClient<IAiService, AiService>();


var app = builder.Build();

// ---------------------------
// HTTP Request Pipeline
// ---------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------------------------
// Seed Roles
// ---------------------------

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // اگر از Migration استفاده می‌کنید، این درست‌تر از EnsureCreated است
        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "User" }
            );

            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Program");

        logger.LogError(ex, "Error occurred while seeding the database.");
    }
}

app.Run();
