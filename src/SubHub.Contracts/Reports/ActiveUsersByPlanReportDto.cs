namespace SubHub.Contracts.Reports;

public class ActiveUsersByPlanReportDto
{
    public int TotalActiveUsers { get; set; }
    public int TotalActiveSubscriptions { get; set; }
    public List<string> HistoricalLabels { get; set; } = new();
    public List<ActiveUsersPerPlanDto> Plans { get; set; } = new();
}

public class ActiveUsersPerPlanDto
{
    public long PlanId { get; set; }
    public string PlanTitle { get; set; } = string.Empty;
    public int ActiveUsers { get; set; }
    public int ActiveSubscriptions { get; set; }
    public List<int> HistoricalActiveUsers { get; set; } = new();
}
