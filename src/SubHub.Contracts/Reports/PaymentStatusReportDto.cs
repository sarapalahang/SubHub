namespace SubHub.Contracts.Reports;

public class PaymentStatusReportDto
{
    public int TotalPayments { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PaymentStatusBreakdownDto> Statuses { get; set; } = new();
}

public class PaymentStatusBreakdownDto
{
    public string StatusCode { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}
