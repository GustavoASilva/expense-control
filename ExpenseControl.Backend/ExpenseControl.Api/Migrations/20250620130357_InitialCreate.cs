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
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IconName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ColorCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false)
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
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
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
                columns: new[] { "Id", "ColorCode", "Description", "IconName", "Name", "Type" },
                values: new object[,]
                {
                    { new Guid("6bccc921-a2f2-4a81-b162-aa71e9dc64cb"), "#32CD32", "Groceries, dining out, etc.", "food", "Food", "Expense" },
                    { new Guid("8b307833-0802-4a50-990a-acd0df4f95db"), "#FFD700", "Electricity, water, internet, etc.", "bolt", "Utilities", "Expense" },
                    { new Guid("a0802a65-3cfb-47fd-bbd3-b18cb1c79f50"), "#FF8C00", "Rent, mortgage, repairs, etc.", "home", "Housing", "Expense" },
                    { new Guid("c7551968-2c51-427d-a4f1-80ce3a4e6347"), "#4682B4", "Independent contractor income", "briefcase", "Freelance", "Income" },
                    { new Guid("cdcdadce-8f78-4c89-ac76-ea6e837dc7f1"), "#228B22", "Regular employment income", "wallet", "Salary", "Income" },
                    { new Guid("d70fd34e-eac3-4f1e-a96d-7edc8173beda"), "#4169E1", "Car, public transit, fuel, etc.", "car", "Transportation", "Expense" },
                    { new Guid("e1cb705a-05ff-440a-aae1-33242e9689a4"), "#FF69B4", "Medical expenses, insurance, etc.", "medical", "Healthcare", "Expense" },
                    { new Guid("e2da5652-4a3d-45db-8a59-7dd7c249f92e"), "#9370DB", "Dividends, interest, capital gains", "chart-line", "Investments", "Income" }
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
