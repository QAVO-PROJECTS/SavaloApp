using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SavaloApp.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleProviderIdForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrencyAccounts_AspNetUsers_UserId1",
                table: "CurrencyAccounts");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyAccounts_UserId1",
                table: "CurrencyAccounts");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "CurrencyAccounts");

            migrationBuilder.AddColumn<string>(
                name: "GoogleProviderId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleProviderId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "CurrencyAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_UserId1",
                table: "CurrencyAccounts",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrencyAccounts_AspNetUsers_UserId1",
                table: "CurrencyAccounts",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
