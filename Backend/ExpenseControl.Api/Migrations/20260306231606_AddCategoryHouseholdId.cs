using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseControl.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryHouseholdId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_Name_Type",
                table: "Categories");

            migrationBuilder.AddColumn<Guid>(
                name: "HouseholdId",
                table: "Categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3feb665e-56c5-4251-bda6-d665dbda65d3"),
                column: "HouseholdId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("51630c3e-35be-4b55-87a9-68ef640f772c"),
                column: "HouseholdId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("683eba8b-531b-465e-8be4-584e85abdfb8"),
                column: "HouseholdId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("92974cbf-e03c-4f33-9e36-9f9f0a97523e"),
                column: "HouseholdId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a48cde78-354e-4d5c-9159-cf28368fcaca"),
                column: "HouseholdId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ac800e65-9ae2-4274-8c8a-ae4658345c99"),
                column: "HouseholdId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c7e8d65f-f4b2-4162-ba57-04f52fb16d51"),
                column: "HouseholdId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("eb318b9d-aafc-421d-8041-64c6889ffc3c"),
                column: "HouseholdId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_HouseholdId",
                table: "Categories",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name_Type_HouseholdId",
                table: "Categories",
                columns: new[] { "Name", "Type", "HouseholdId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Households_HouseholdId",
                table: "Categories",
                column: "HouseholdId",
                principalTable: "Households",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Households_HouseholdId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_HouseholdId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name_Type_HouseholdId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "HouseholdId",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name_Type",
                table: "Categories",
                columns: new[] { "Name", "Type" },
                unique: true);
        }
    }
}
