using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class FilterRequestTypeDefaultDepartmentIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RequestTypeDefaultDepartments_BranchId_RequestType",
                schema: "master",
                table: "RequestTypeDefaultDepartments");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTypeDefaultDepartments_BranchId_RequestType",
                schema: "master",
                table: "RequestTypeDefaultDepartments",
                columns: new[] { "BranchId", "RequestType" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RequestTypeDefaultDepartments_BranchId_RequestType",
                schema: "master",
                table: "RequestTypeDefaultDepartments");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTypeDefaultDepartments_BranchId_RequestType",
                schema: "master",
                table: "RequestTypeDefaultDepartments",
                columns: new[] { "BranchId", "RequestType" },
                unique: true);
        }
    }
}
