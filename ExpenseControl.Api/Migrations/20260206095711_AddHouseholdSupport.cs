using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseControl.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseholdSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_CategoryId_Month_Year",
                table: "Budgets");

            migrationBuilder.AddColumn<Guid>(
                name: "HouseholdId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "HouseholdId",
                table: "Budgets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Households",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Households", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_HouseholdId",
                table: "Transactions",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CategoryId_Month_Year_HouseholdId",
                table: "Budgets",
                columns: new[] { "CategoryId", "Month", "Year", "HouseholdId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_HouseholdId",
                table: "Budgets",
                column: "HouseholdId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Households_HouseholdId",
                table: "Budgets",
                column: "HouseholdId",
                principalTable: "Households",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Households_HouseholdId",
                table: "Transactions",
                column: "HouseholdId",
                principalTable: "Households",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Households_HouseholdId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Households_HouseholdId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Households");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_HouseholdId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_CategoryId_Month_Year_HouseholdId",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_HouseholdId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "HouseholdId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "HouseholdId",
                table: "Budgets");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CategoryId_Month_Year",
                table: "Budgets",
                columns: new[] { "CategoryId", "Month", "Year" },
                unique: true);
        }
    }
}
