using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentsToEngineerRequestActivite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comments",
                schema: "business",
                table: "EngineerRequestActivites",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                schema: "business",
                table: "EngineerRequestActivites");
        }
    }
}
