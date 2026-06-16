using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedToFinancialClearance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToId",
                schema: "business",
                table: "FinancialClearances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearances_AssignedToId",
                schema: "business",
                table: "FinancialClearances",
                column: "AssignedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialClearances_Engineers_AssignedToId",
                schema: "business",
                table: "FinancialClearances",
                column: "AssignedToId",
                principalSchema: "master",
                principalTable: "Engineers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialClearances_Engineers_AssignedToId",
                schema: "business",
                table: "FinancialClearances");

            migrationBuilder.DropIndex(
                name: "IX_FinancialClearances_AssignedToId",
                schema: "business",
                table: "FinancialClearances");

            migrationBuilder.DropColumn(
                name: "AssignedToId",
                schema: "business",
                table: "FinancialClearances");
        }
    }
}
