using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubHub.Application.Interfaces;
using SubHub.Infrastructure.Data;
using SubHub.Infrastructure.Services;

namespace SubHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ثبت DbContext به همراه تعیین MigrationsAssembly
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // ثبت درگاه پرداخت
        services.AddHttpClient<IPaymentGatewayService, ZarinPalPaymentGatewayService>();
        services.AddScoped<ISmsService, OtpSmsService>();


        return services;
    }
}
