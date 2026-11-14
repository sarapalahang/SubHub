namespace SubHub.Contracts.Reports;

public class CustomerActivityReportDto
{
    public int RegistrationsThisMonth { get; set; }
    public int FirstTimeBuyersThisMonth { get; set; }
    public int RenewalCustomersThisMonth { get; set; }
    public List<string> MonthlyLabels { get; set; } = new();
    public List<int> NewRegistrations { get; set; } = new();
    public List<int> FirstTimeBuyers { get; set; } = new();
    public List<int> RenewalCustomers { get; set; } = new();
    public List<decimal> FirstTimeBuyerPercentages { get; set; } = new();
    public List<decimal> RenewalPercentages { get; set; } = new();
}
