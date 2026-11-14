using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubHub.Infrastructure.Migrations;

public partial class AddReportingAnalytics : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PaymentGateway",
            table: "Invoices",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: false,
            defaultValue: "ZarinPal");

        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAtUtc",
            table: "Users",
            type: "datetime2",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ExpiryNotificationLogs",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SubscriptionId = table.Column<long>(type: "bigint", nullable: false),
                Channel = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                Recipient = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                DaysRemaining = table.Column<int>(type: "int", nullable: false),
                AttemptedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExpiryNotificationLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_ExpiryNotificationLogs_Subscriptions_SubscriptionId",
                    column: x => x.SubscriptionId,
                    principalTable: "Subscriptions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ExpiryNotificationLogs_AttemptedAtUtc",
            table: "ExpiryNotificationLogs",
            column: "AttemptedAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_ExpiryNotificationLogs_Channel_Status_AttemptedAtUtc",
            table: "ExpiryNotificationLogs",
            columns: new[] { "Channel", "Status", "AttemptedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_ExpiryNotificationLogs_SubscriptionId",
            table: "ExpiryNotificationLogs",
            column: "SubscriptionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ExpiryNotificationLogs");
        migrationBuilder.DropColumn(name: "PaymentGateway", table: "Invoices");
        migrationBuilder.DropColumn(name: "CreatedAtUtc", table: "Users");
    }
}
