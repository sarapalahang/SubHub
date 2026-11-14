using Microsoft.Extensions.DependencyInjection;
using SubHub.Application.Interfaces;
using SubHub.Application.Services;

namespace SubHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ISubscriptionExpiryService, SubscriptionExpiryService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
