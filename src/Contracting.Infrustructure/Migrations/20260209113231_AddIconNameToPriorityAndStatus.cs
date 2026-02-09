using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIconNameToPriorityAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "iconName",
                schema: "master",
                table: "Statuses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "iconName",
                schema: "master",
                table: "Priorities",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "iconName",
                schema: "master",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "iconName",
                schema: "master",
                table: "Priorities");
        }
    }
}
