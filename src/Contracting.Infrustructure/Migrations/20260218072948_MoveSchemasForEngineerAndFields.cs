using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveSchemasForEngineerAndFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SpecialFields",
                newName: "SpecialFields",
                newSchema: "master");

            migrationBuilder.RenameTable(
                name: "ProjectSpecialFields",
                newName: "ProjectSpecialFields",
                newSchema: "master");

            migrationBuilder.RenameTable(
                name: "EngineerRequestSpecialFieldValues",
                newName: "EngineerRequestSpecialFieldValues",
                newSchema: "business");

            migrationBuilder.RenameTable(
                name: "EngineerProjects",
                newName: "EngineerProjects",
                newSchema: "master");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "master",
                table: "SpecialFields",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "fieldType",
                schema: "master",
                table: "SpecialFields",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "value",
                schema: "business",
                table: "EngineerRequestSpecialFieldValues",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SpecialFields",
                schema: "master",
                newName: "SpecialFields");

            migrationBuilder.RenameTable(
                name: "ProjectSpecialFields",
                schema: "master",
                newName: "ProjectSpecialFields");

            migrationBuilder.RenameTable(
                name: "EngineerRequestSpecialFieldValues",
                schema: "business",
                newName: "EngineerRequestSpecialFieldValues");

            migrationBuilder.RenameTable(
                name: "EngineerProjects",
                schema: "master",
                newName: "EngineerProjects");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "SpecialFields",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "fieldType",
                table: "SpecialFields",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "value",
                table: "EngineerRequestSpecialFieldValues",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
