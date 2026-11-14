using SubHub.Contracts.Plans;

namespace SubHub.Application.Interfaces;

public interface IPlanService
{
    // ۱. دریافت لیست تمام پلن‌های فعال برای نمایش به کاربر
    Task<List<PlanResponse>> GetActivePlansAsync(CancellationToken cancellationToken = default);

    // مدیریت پلن‌ها در پنل راهبر
    Task<List<PlanResponse>> GetAllPlansAsync(CancellationToken cancellationToken = default);
    Task<PlanResponse?> GetPlanByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PlanResponse> CreatePlanAsync(CreatePlanRequest request, CancellationToken cancellationToken = default);
    Task<PlanResponse?> UpdatePlanAsync(
        long id,
        CreatePlanRequest request,
        bool isActive,
        CancellationToken cancellationToken = default);
    Task<bool> DeactivatePlanAsync(long id, CancellationToken cancellationToken = default);
}
