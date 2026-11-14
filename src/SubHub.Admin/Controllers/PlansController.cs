using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubHub.Admin.Models;
using SubHub.Application.Interfaces;
using SubHub.Contracts.Plans;
using SubHub.Domain.Enums;

namespace SubHub.Admin.Controllers;

[Authorize(AuthenticationSchemes = "SubHubAdminCookie")]
public class PlansController : Controller
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var plans = await _planService.GetAllPlansAsync(cancellationToken);
        return View(plans.Select(ToViewModel).ToList());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new PlanManagementViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlanManagementViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _planService.CreatePlanAsync(ToRequest(model), cancellationToken);
        TempData["SuccessMessage"] = "پلن جدید با موفقیت اضافه شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var plan = await _planService.GetPlanByIdAsync(id, cancellationToken);
        if (plan is null)
            return NotFound();

        return View(ToViewModel(plan));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, PlanManagementViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var updatedPlan = await _planService.UpdatePlanAsync(
            id,
            ToRequest(model),
            model.IsActive,
            cancellationToken);

        if (updatedPlan is null)
            return NotFound();

        TempData["SuccessMessage"] = "تغییرات پلن ذخیره شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deactivated = await _planService.DeactivatePlanAsync(id, cancellationToken);
        if (!deactivated)
            return NotFound();

        TempData["SuccessMessage"] = "پلن از فهرست فروش غیرفعال شد و سوابق آن حفظ شد.";
        return RedirectToAction(nameof(Index));
    }

    private static CreatePlanRequest ToRequest(PlanManagementViewModel model)
    {
        return new CreatePlanRequest
        {
            Title = model.Title,
            Description = model.Description,
            Price = model.Price,
            DurationType = model.DurationType,
            DurationInDays = model.DurationInDays
        };
    }

    private static PlanManagementViewModel ToViewModel(PlanResponse plan)
    {
        return new PlanManagementViewModel
        {
            Id = plan.Id,
            Title = plan.Title,
            Description = plan.Description,
            Price = plan.Price,
            DurationType = plan.DurationType,
            DurationInDays = plan.DurationInDays,
            IsActive = plan.IsActive
        };
    }
}
