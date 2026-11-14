# SubHub

SubHub is a subscription management platform built with ASP.NET Core MVC and SQL Server. It includes a customer-facing subscription and payment flow, an administrator dashboard, subscription plan management, sales and payment reports, cancellation tracking, and Hangfire jobs for invoice reconciliation and subscription-expiry notifications.

## Technology

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core and SQL Server
- Hangfire recurring jobs
- Chart.js dashboard and reports
- ZarinPal payment gateway integration
- Kavenegar SMS and SMTP email integrations

## Projects

- `src/SubHub.Web` — customer site, account, subscriptions, checkout, and background jobs
- `src/SubHub.Admin` — administrator sign-in, plan CRUD, dashboard, and reports
- `src/SubHub.Application` — application services and interfaces
- `src/SubHub.Domain` — domain entities and enums
- `src/SubHub.Infrastructure` — database, payment, SMS, email, and PDF implementations
- `src/SubHub.Contracts` — request, response, and report models

## Run locally

1. Install the .NET 10 SDK and SQL Server.
2. Set a local SQL Server connection string using User Secrets for both `SubHub.Web` and `SubHub.Admin`.
3. Configure the payment, SMS, email, and administrator credentials using User Secrets. Do not put real credentials in `appsettings.json` or commit them.
4. Apply the EF Core migrations from the repository root:

   ```powershell
   dotnet ef database update --project src/SubHub.Infrastructure/SubHub.Infrastructure.csproj --startup-project src/SubHub.Web/SubHub.Web.csproj
   dotnet ef database update --project src/SubHub.Infrastructure/SubHub.Infrastructure.csproj --startup-project src/SubHub.Admin/SubHub.Admin.csproj
   ```

5. Run `SubHub.Web` for the customer site and Hangfire jobs. Run `SubHub.Admin` for the management dashboard.

Example User Secrets commands (replace the example values locally):

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-local-sql-server-connection-string>" --project src/SubHub.Web/SubHub.Web.csproj
dotnet user-secrets set "Kavenegar:ApiKey" "<your-kavenegar-key>" --project src/SubHub.Web/SubHub.Web.csproj
dotnet user-secrets set "Kavenegar:Template" "<your-kavenegar-template>" --project src/SubHub.Web/SubHub.Web.csproj
dotnet user-secrets set "ZarinPal:MerchantId" "<your-merchant-id>" --project src/SubHub.Web/SubHub.Web.csproj
dotnet user-secrets set "SmtpSettings:Host" "<your-smtp-host>" --project src/SubHub.Web/SubHub.Web.csproj
dotnet user-secrets set "SmtpSettings:UserName" "<your-smtp-username>" --project src/SubHub.Web/SubHub.Web.csproj
dotnet user-secrets set "SmtpSettings:Password" "<your-smtp-password>" --project src/SubHub.Web/SubHub.Web.csproj
dotnet user-secrets set "SmtpSettings:From" "<your-sender-address>" --project src/SubHub.Web/SubHub.Web.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-local-sql-server-connection-string>" --project src/SubHub.Admin/SubHub.Admin.csproj
dotnet user-secrets set "AdminCredentials:Username" "<admin-username>" --project src/SubHub.Admin/SubHub.Admin.csproj
dotnet user-secrets set "AdminCredentials:Password" "<strong-admin-password>" --project src/SubHub.Admin/SubHub.Admin.csproj
```

Set production secrets through the hosting provider's secret store or environment variables. Rotate any credential that was previously stored in source files before deploying.

## Notes

- The dashboard and reports use the database's available history. Older records may not have all analytics fields populated.
- The expiry-notification report records attempts from the time notification logging is enabled.
- OTP challenges are kept in process memory. Use a shared distributed cache when running multiple web instances.
- Chart.js is loaded from a CDN, so report charts require network access unless the library is hosted locally.
