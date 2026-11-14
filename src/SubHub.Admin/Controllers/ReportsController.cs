using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubHub.Application.Interfaces;

namespace SubHub.Admin.Controllers;

[Authorize(AuthenticationSchemes = "SubHubAdminCookie")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<IActionResult> Sales(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetSubscriptionCommerceReportAsync(cancellationToken);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> Revenue(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetSubscriptionCommerceReportAsync(cancellationToken);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> ActiveUsersByPlan(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetActiveUsersByPlanReportAsync(cancellationToken);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> PlanDistribution(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetSubscriptionCommerceReportAsync(cancellationToken);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> NewAndRenewals(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetCustomerActivityReportAsync(cancellationToken);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> Payments(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetPaymentStatusReportAsync(cancellationToken);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> Cancellations(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetDashboardReportAsync(cancellationToken);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> ExpiryNotifications(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetExpiryNotificationReportAsync(cancellationToken);
        return View(report);
    }
}
