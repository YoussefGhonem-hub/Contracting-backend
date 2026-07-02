using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameIsListToListGroupKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsList",
                schema: "master",
                table: "DepartmentSpecialFields");

            migrationBuilder.AddColumn<string>(
                name: "ListGroupKey",
                schema: "master",
                table: "DepartmentSpecialFields",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListGroupKey",
                schema: "master",
                table: "DepartmentSpecialFields");

            migrationBuilder.AddColumn<bool>(
                name: "IsList",
                schema: "master",
                table: "DepartmentSpecialFields",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
