using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.Sql(@"
;WITH RankedBuyerEmails AS
(
    SELECT
        [UserId],
        [BuyerEmail],
        ROW_NUMBER() OVER (PARTITION BY [UserId] ORDER BY [CreatedAtUtc] DESC, [Id] DESC) AS [RowNumber]
    FROM [Invoices]
    WHERE [BuyerEmail] IS NOT NULL AND LTRIM(RTRIM([BuyerEmail])) <> ''
)
UPDATE [Users]
SET [Email] = [RankedBuyerEmails].[BuyerEmail]
FROM [Users]
INNER JOIN [RankedBuyerEmails] ON [Users].[Id] = [RankedBuyerEmails].[UserId]
WHERE [RankedBuyerEmails].[RowNumber] = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");
        }
    }
}
