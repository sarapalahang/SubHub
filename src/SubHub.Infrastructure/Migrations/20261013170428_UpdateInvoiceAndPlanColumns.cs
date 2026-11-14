using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInvoiceAndPlanColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Plans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Invoices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Invoices");
        }
    }
}
