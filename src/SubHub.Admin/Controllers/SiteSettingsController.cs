using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubHub.Application.Interfaces;
using SubHub.Domain.Entities;

namespace SubHub.Admin.Controllers;

[Authorize(AuthenticationSchemes = "SubHubAdminCookie", Roles = "Admin")]
public class SiteSettingsController : Controller
{
    private readonly IApplicationDbContext _context;

    public SiteSettingsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var settings = await _context.SiteSettings.FirstOrDefaultAsync(cancellationToken)
            ?? new SiteSetting();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SiteSetting model, CancellationToken cancellationToken)
    {
        var settings = await _context.SiteSettings.FirstOrDefaultAsync(cancellationToken);
        if (settings is null)
        {
            settings = new SiteSetting();
            _context.SiteSettings.Add(settings);
        }

        settings.EnableSmsNotification = model.EnableSmsNotification;
        settings.EnableEmailNotification = model.EnableEmailNotification;
        settings.EnablePdfAttachment = model.EnablePdfAttachment;

        await _context.SaveChangesAsync(cancellationToken);
        TempData["SuccessMessage"] = "تنظیمات با موفقیت ذخیره شد.";
        return RedirectToAction(nameof(Index));
    }
}
