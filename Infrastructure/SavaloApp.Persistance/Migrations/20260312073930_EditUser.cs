using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SavaloApp.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class EditUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppleProviderId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthProvider",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppleProviderId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OAuthProvider",
                table: "AspNetUsers");
        }
    }
}
