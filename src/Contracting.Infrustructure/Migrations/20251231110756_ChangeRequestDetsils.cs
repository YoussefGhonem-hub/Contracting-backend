using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRequestDetsils : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "EngineerRequests");

            migrationBuilder.RenameColumn(
                name: "NoteDate",
                table: "EngineerRequests",
                newName: "startDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "endDate",
                table: "EngineerRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "endDate",
                table: "EngineerRequests");

            migrationBuilder.RenameColumn(
                name: "startDate",
                table: "EngineerRequests",
                newName: "NoteDate");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "EngineerRequests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
