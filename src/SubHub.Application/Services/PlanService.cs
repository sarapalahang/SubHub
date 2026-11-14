using Microsoft.EntityFrameworkCore;
using SubHub.Application.Interfaces;
using SubHub.Contracts.Plans;
using SubHub.Domain.Entities;

namespace SubHub.Application.Services;

public class PlanService : IPlanService
{
    private readonly IApplicationDbContext _context;

    public PlanService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlanResponse>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Plans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new PlanResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                DurationType = p.DurationType,
                DurationInDays = p.DurationInDays,
                IsActive = p.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PlanResponse>> GetAllPlansAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Plans
            .AsNoTracking()
            .OrderBy(plan => plan.Id)
            .Select(plan => new PlanResponse
            {
                Id = plan.Id,
                Title = plan.Title,
                Description = plan.Description,
                Price = plan.Price,
                DurationType = plan.DurationType,
                DurationInDays = plan.DurationInDays,
                IsActive = plan.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PlanResponse?> GetPlanByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        return plan is null ? null : MapPlan(plan);
    }

    public async Task<PlanResponse> CreatePlanAsync(CreatePlanRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var plan = new Plan
        {
            Title = request.Title.Trim(),
            Name = request.Title.Trim(),
            Description = NormalizeDescription(request.Description),
            Price = request.Price,
            DurationType = request.DurationType,
            DurationInDays = request.DurationInDays,
            IsActive = true
        };

        _context.Plans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        return MapPlan(plan);
    }

    public async Task<PlanResponse?> UpdatePlanAsync(
        long id,
        CreatePlanRequest request,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var plan = await _context.Plans
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (plan is null)
            return null;

        plan.Title = request.Title.Trim();
        plan.Name = request.Title.Trim();
        plan.Description = NormalizeDescription(request.Description);
        plan.Price = request.Price;
        plan.DurationType = request.DurationType;
        plan.DurationInDays = request.DurationInDays;
        plan.IsActive = isActive;

        await _context.SaveChangesAsync(cancellationToken);
        return MapPlan(plan);
    }

    public async Task<bool> DeactivatePlanAsync(long id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.Plans
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (plan is null)
            return false;

        plan.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ValidateRequest(CreatePlanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var title = request.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("عنوان پلن الزامی است.", nameof(request));
        if (title.Length > 150)
            throw new ArgumentException("عنوان پلن نمی‌تواند بیشتر از ۱۵۰ نویسه باشد.", nameof(request));
        if (request.Description?.Length > 500)
            throw new ArgumentException("توضیحات پلن نمی‌تواند بیشتر از ۵۰۰ نویسه باشد.", nameof(request));
        if (request.Price <= 0)
            throw new ArgumentException("قیمت پلن باید بیشتر از صفر باشد.", nameof(request));
        if (request.Price > 9_999_999_999_999_999.99m)
            throw new ArgumentException("قیمت پلن از محدوده قابل ذخیره بیشتر است.", nameof(request));
        if (!Enum.IsDefined(request.DurationType))
            throw new ArgumentException("نوع دوره انتخاب‌شده معتبر نیست.", nameof(request));
        if (request.DurationInDays is < 1 or > 36500)
            throw new ArgumentException("مدت اعتبار باید بین ۱ تا ۳۶۵۰۰ روز باشد.", nameof(request));
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static PlanResponse MapPlan(Plan plan)
    {
        return new PlanResponse
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
