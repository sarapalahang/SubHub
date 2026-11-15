using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobileLoginOtp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToPaymentLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId1",
                table: "PaymentLogs",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLogs_UserId1",
                table: "PaymentLogs",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentLogs_Users_UserId1",
                table: "PaymentLogs",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentLogs_Users_UserId1",
                table: "PaymentLogs");

            migrationBuilder.DropIndex(
                name: "IX_PaymentLogs_UserId1",
                table: "PaymentLogs");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PaymentLogs");
        }
    }
}
