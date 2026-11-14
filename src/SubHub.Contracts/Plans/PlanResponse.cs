using SubHub.Domain.Enums;

namespace SubHub.Contracts.Plans;

public class PlanResponse
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public PlanDurationType DurationType { get; set; }
    public int DurationInDays { get; set; }
    public bool IsActive { get; set; }
}
