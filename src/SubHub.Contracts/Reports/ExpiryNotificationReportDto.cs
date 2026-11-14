namespace SubHub.Contracts.Reports;

public class ExpiryNotificationReportDto
{
    public int SmsSucceeded { get; set; }
    public int SmsFailed { get; set; }
    public int EmailSucceeded { get; set; }
    public int EmailFailed { get; set; }
    public List<string> MonthlyLabels { get; set; } = new();
    public List<int> MonthlySms { get; set; } = new();
    public List<int> MonthlyEmail { get; set; } = new();
    public List<NotificationChannelStatusDto> Statuses { get; set; } = new();
}

public class NotificationChannelStatusDto
{
    public string ChannelCode { get; set; } = string.Empty;
    public string ChannelLabel { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}
