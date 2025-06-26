using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ExpenseControl.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IconName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "IconName", "Name", "Type" },
                values: new object[,]
                {
                    { new Guid("09860853-4c8d-45af-b2f8-c13252feeb28"), "Rent, mortgage, repairs, etc.", "house", "Housing", "Expense" },
                    { new Guid("6129f445-8088-413c-a11f-c5ea70820e73"), "Regular employment income", "wallet2", "Salary", "Income" },
                    { new Guid("788cf435-f6f7-4bee-8954-e8f5a42bb4a6"), "Car, public transit, fuel, etc.", "car-front", "Transportation", "Expense" },
                    { new Guid("a62e5b4c-2202-4dea-892c-bc439d5041c3"), "Medical expenses, insurance, etc.", "heart-pulse", "Healthcare", "Expense" },
                    { new Guid("b5f100f7-990c-4dbb-8a49-9868b4694d4d"), "Groceries, dining out, etc.", "cart", "Food", "Expense" },
                    { new Guid("eaba9ca2-c6c8-4fb2-85a7-3b31813cc8f9"), "Electricity, water, internet, etc.", "lightning", "Utilities", "Expense" },
                    { new Guid("f841ed1c-0548-4a73-aaf2-25f274d8d603"), "Independent contractor income", "briefcase", "Freelance", "Income" },
                    { new Guid("f8af4340-31b2-4a7f-9f11-f08e3ed760ce"), "Dividends, interest, capital gains", "graph-up-arrow", "Investments", "Income" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name_Type",
                table: "Categories",
                columns: new[] { "Name", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
