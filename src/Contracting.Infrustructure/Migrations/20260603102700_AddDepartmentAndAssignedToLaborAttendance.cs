using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentAndAssignedToLaborAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToId",
                schema: "business",
                table: "LaborAttendanceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                schema: "business",
                table: "LaborAttendanceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceRequests_AssignedToId",
                schema: "business",
                table: "LaborAttendanceRequests",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceRequests_DepartmentId",
                schema: "business",
                table: "LaborAttendanceRequests",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_LaborAttendanceRequests_Departments_DepartmentId",
                schema: "business",
                table: "LaborAttendanceRequests",
                column: "DepartmentId",
                principalSchema: "master",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LaborAttendanceRequests_Engineers_AssignedToId",
                schema: "business",
                table: "LaborAttendanceRequests",
                column: "AssignedToId",
                principalSchema: "master",
                principalTable: "Engineers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LaborAttendanceRequests_Departments_DepartmentId",
                schema: "business",
                table: "LaborAttendanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LaborAttendanceRequests_Engineers_AssignedToId",
                schema: "business",
                table: "LaborAttendanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_LaborAttendanceRequests_AssignedToId",
                schema: "business",
                table: "LaborAttendanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_LaborAttendanceRequests_DepartmentId",
                schema: "business",
                table: "LaborAttendanceRequests");

            migrationBuilder.DropColumn(
                name: "AssignedToId",
                schema: "business",
                table: "LaborAttendanceRequests");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "business",
                table: "LaborAttendanceRequests");
        }
    }
}
