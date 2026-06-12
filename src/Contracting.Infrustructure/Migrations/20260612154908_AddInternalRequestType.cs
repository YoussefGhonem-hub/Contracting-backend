using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInternalRequestType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestType",
                schema: "business",
                table: "EngineerRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestType",
                schema: "business",
                table: "EngineerRequests");
        }
    }
}
