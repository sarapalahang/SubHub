using Hangfire;
using SubHub.Application;
using SubHub.Application.Interfaces;
using SubHub.Application.Services;
using SubHub.Infrastructure;
using SubHub.Infrastructure.Services;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// اضافه کردن سرویس‌های MVC
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

// ثبت سرویس‌های لایه‌های معماری پیازی
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<ISmsService, OtpSmsService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPdfService, PdfService>();

// تنظیم احراز هویت با کوکی
builder.Services.AddAuthentication("SubHubCookie")
    .AddCookie("SubHubCookie", options =>
    {
        options.Cookie.Name = "SubHub.AuthCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/Account/Login"; // آدرس صفحه لاگین
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // کوکی ۳۰ روز معتبر بماند
    });

// -------------------------------------------------------------
// ۱. پیکربندی و ثبت سرویس‌های Hangfire
// -------------------------------------------------------------
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

// اضافه کردن سرور Hangfire برای پردازش Background Jobها
builder.Services.AddHangfireServer();

var app = builder.Build();

// تنظیمات پایپ‌لاین
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // اول شناسایی کاربر (احراز هویت)
app.UseAuthorization();  // بعد بررسی دسترسی‌ها

// -------------------------------------------------------------
// ۲. فعال‌سازی داشبورد Hangfire و تعریف Recurring Job
// -------------------------------------------------------------
app.UseHangfireDashboard("/hangfire");

// تعریف جاب دوره‌ای: استعلام فاکتورهای معلق هر ۱۰ دقیقه یک‌بار
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<IInvoiceService>(
        "reconcile-pending-invoices",
        service => service.ReconcilePendingInvoicesAsync(),
        Cron.MinuteInterval(10));

    recurringJobManager.AddOrUpdate<ISubscriptionExpiryService>(
        "process-subscription-expirations",
        service => service.ProcessSubscriptionExpirationsAsync(),
        Cron.Daily());
}

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
