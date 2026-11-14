using SubHub.Contracts.Reports;

namespace SubHub.Application.Interfaces;

public interface IReportService
{
    Task<DashboardReportDto> GetDashboardReportAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionCommerceReportDto> GetSubscriptionCommerceReportAsync(CancellationToken cancellationToken = default);
    Task<ActiveUsersByPlanReportDto> GetActiveUsersByPlanReportAsync(CancellationToken cancellationToken = default);
    Task<CustomerActivityReportDto> GetCustomerActivityReportAsync(CancellationToken cancellationToken = default);
    Task<PaymentStatusReportDto> GetPaymentStatusReportAsync(CancellationToken cancellationToken = default);
    Task<ExpiryNotificationReportDto> GetExpiryNotificationReportAsync(CancellationToken cancellationToken = default);
}
