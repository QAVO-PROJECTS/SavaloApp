using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SavaloApp.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class EditCategoryAndGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GoalSectionId",
                table: "Goals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CategorySectionId",
                table: "Categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Goals_GoalSectionId",
                table: "Goals",
                column: "GoalSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategorySectionId",
                table: "Categories",
                column: "CategorySectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_CategorySections_CategorySectionId",
                table: "Categories",
                column: "CategorySectionId",
                principalTable: "CategorySections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_GoalSections_GoalSectionId",
                table: "Goals",
                column: "GoalSectionId",
                principalTable: "GoalSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_CategorySections_CategorySectionId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Goals_GoalSections_GoalSectionId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Goals_GoalSectionId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CategorySectionId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "GoalSectionId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "CategorySectionId",
                table: "Categories");
        }
    }
}
