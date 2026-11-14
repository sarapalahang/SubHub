using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionExpiryNotificationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastExpiryEmailNotificationDateUtc",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExpirySmsNotificationDateUtc",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastExpiryEmailNotificationDateUtc",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "LastExpirySmsNotificationDateUtc",
                table: "Subscriptions");
        }
    }
}
