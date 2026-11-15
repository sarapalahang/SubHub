# SubHub

SubHub is a portfolio project for managing subscription plans, customer subscriptions, invoices, and payments. It includes a customer-facing ASP.NET Core MVC application, a separate admin portal, reporting dashboards, and background jobs for invoice reconciliation and subscription-expiry notifications.

## Features

- Customer sign-in and subscription purchase flow
- Subscription plan management in the admin portal
- Invoice and payment workflows with ZarinPal integration
- Sales, revenue, payment, active-user, cancellation, and expiry-notification reports
- Hangfire jobs for reconciling pending invoices and notifying users about expiring subscriptions
- SMS and email notification integrations

## Architecture

The solution uses a layered design inspired by Onion Architecture. The domain is kept separate from the web applications, while application services and infrastructure implementations are organized into their own projects.

- **Domain** contains the core entities and enums.
- **Application** contains business services and application-facing interfaces.
- **Infrastructure** implements database access and external integrations such as payments, SMS, email, and PDF generation.
- **Contracts** contains shared request, response, and reporting models.
- **Web** is the customer-facing site and hosts the background jobs.
- **Admin** is the management portal for plans, settings, and reports.

## Technology

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core and SQL Server
- Hangfire recurring jobs
- Chart.js dashboard and reports
- ZarinPal payment gateway integration
- Kavenegar SMS and SMTP email integrations
- QuestPDF for PDF generation

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
