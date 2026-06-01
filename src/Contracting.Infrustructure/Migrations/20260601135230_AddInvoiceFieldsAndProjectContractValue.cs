using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceFieldsAndProjectContractValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ContractValue",
                schema: "master",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DueDate",
                schema: "client",
                table: "ProjectInvoices",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceNumber",
                schema: "client",
                table: "ProjectInvoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "IssueDate",
                schema: "client",
                table: "ProjectInvoices",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "client",
                table: "ProjectInvoices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractValue",
                schema: "master",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DueDate",
                schema: "client",
                table: "ProjectInvoices");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                schema: "client",
                table: "ProjectInvoices");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                schema: "client",
                table: "ProjectInvoices");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "client",
                table: "ProjectInvoices");
        }
    }
}
