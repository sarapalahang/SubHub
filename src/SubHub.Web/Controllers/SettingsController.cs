using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubHub.Domain.Entities;
using SubHub.Infrastructure.Data;

using Microsoft.AspNetCore.Authorization;

namespace SubHub.Web.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // اولین رکورد موجود را می‌خواند
        var settings = await _context.SiteSettings.FirstOrDefaultAsync();

        // اگر هیچ رکوردی در دیتابیس نبود، یک نمونه جدید می‌سازد
        if (settings == null)
        {
            settings = new SiteSetting();
        }

        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SiteSetting model)
    {
        // بررسی وجود رکورد در دیتابیس
        var settings = await _context.SiteSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            // ساخت اولین رکورد (بدون مقداردهی دستی Id)
            var newSettings = new SiteSetting
            {
                EnableSmsNotification = model.EnableSmsNotification,
                EnableEmailNotification = model.EnableEmailNotification,
                EnablePdfAttachment = model.EnablePdfAttachment
            };
            _context.SiteSettings.Add(newSettings);
        }
        else
        {
            // به روز رسانی رکورد موجود
            settings.EnableSmsNotification = model.EnableSmsNotification;
            settings.EnableEmailNotification = model.EnableEmailNotification;
            settings.EnablePdfAttachment = model.EnablePdfAttachment;
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "تنظیمات با موفقیت ذخیره شد.";

        return RedirectToAction(nameof(Index));
    }
}
