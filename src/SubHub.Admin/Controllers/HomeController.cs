using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubHub.Application.Interfaces;
using SubHub.Admin.Models;
using System.Diagnostics;

namespace SubHub.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly IReportService _reportService;

        public HomeController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [Authorize(AuthenticationSchemes = "SubHubAdminCookie")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var report = await _reportService.GetDashboardReportAsync(cancellationToken);
                return View(report);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The browser disconnected or navigated away while the dashboard query was running.
                return new EmptyResult();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
