using SubHub.Domain.Common;
using SubHub.Domain.Enums;

namespace SubHub.Domain.Entities;

public class Plan : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal Price { get; set; }
    public PlanDurationType DurationType { get; set; }
    public int DurationInDays { get; set; }

    public bool IsActive { get; set; } = true;
    public string Name { get; set; } = string.Empty;
}
