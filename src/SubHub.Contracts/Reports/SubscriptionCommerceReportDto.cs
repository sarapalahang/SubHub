namespace SubHub.Contracts.Reports;

public class SubscriptionCommerceReportDto
{
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CurrentMonthSales { get; set; }
    public int PreviousMonthSales { get; set; }
    public int CurrentYearSales { get; set; }
    public decimal CurrentMonthRevenue { get; set; }
    public decimal PreviousMonthRevenue { get; set; }
    public decimal CurrentYearRevenue { get; set; }
    public int TodaySales { get; set; }
    public int ThisWeekSales { get; set; }
    public List<string> DailyLabels { get; set; } = new();
    public List<int> DailySales { get; set; } = new();
    public List<string> WeeklyLabels { get; set; } = new();
    public List<int> WeeklySales { get; set; } = new();
    public List<string> MonthlyLabels { get; set; } = new();
    public List<int> MonthlySales { get; set; } = new();
    public List<decimal> MonthlyRevenue { get; set; } = new();
    public List<string> AnnualLabels { get; set; } = new();
    public List<int> AnnualSales { get; set; } = new();
    public List<PlanCommerceBreakdownDto> PlanBreakdown { get; set; } = new();
    public List<PlanDurationRevenueDto> PlanDurationBreakdown { get; set; } = new();
    public List<GatewayRevenueDto> GatewayBreakdown { get; set; } = new();
}

public class PlanDurationRevenueDto
{
    public string DurationLabel { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal Revenue { get; set; }
}

public class GatewayRevenueDto
{
    public string GatewayName { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal Revenue { get; set; }
}

public class PlanCommerceBreakdownDto
{
    public long PlanId { get; set; }
    public string PlanTitle { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal Revenue { get; set; }
}
