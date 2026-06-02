using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNeedsReceiptConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsReceiptConfirmation",
                schema: "business",
                table: "EngineerRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ItemCode",
                schema: "master",
                table: "ConstructionItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                schema: "master",
                table: "ConstructionItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsReceiptConfirmation",
                schema: "business",
                table: "EngineerRequests");

            migrationBuilder.DropColumn(
                name: "ItemCode",
                schema: "master",
                table: "ConstructionItems");

            migrationBuilder.DropColumn(
                name: "Unit",
                schema: "master",
                table: "ConstructionItems");
        }
    }
}
