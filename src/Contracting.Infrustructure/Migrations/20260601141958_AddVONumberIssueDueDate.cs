using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVONumberIssueDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DueDate",
                schema: "client",
                table: "VariationOrders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "IssueDate",
                schema: "client",
                table: "VariationOrders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VONumber",
                schema: "client",
                table: "VariationOrders",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                schema: "client",
                table: "VariationOrders");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                schema: "client",
                table: "VariationOrders");

            migrationBuilder.DropColumn(
                name: "VONumber",
                schema: "client",
                table: "VariationOrders");
        }
    }
}
