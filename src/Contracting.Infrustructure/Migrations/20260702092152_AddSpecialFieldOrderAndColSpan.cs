using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialFieldOrderAndColSpan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColSpan",
                schema: "master",
                table: "DepartmentSpecialFields",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                schema: "master",
                table: "DepartmentSpecialFields",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColSpan",
                schema: "master",
                table: "DepartmentSpecialFields");

            migrationBuilder.DropColumn(
                name: "Order",
                schema: "master",
                table: "DepartmentSpecialFields");
        }
    }
}
