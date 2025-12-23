using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class fixrelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departmentes_Engineers_DepartmentManagerId",
                table: "Departmentes");

            migrationBuilder.DropForeignKey(
                name: "FK_Engineers_Departmentes_DepartmentId",
                table: "Engineers");

            migrationBuilder.DropIndex(
                name: "IX_Departmentes_DepartmentManagerId",
                table: "Departmentes");

            migrationBuilder.DropColumn(
                name: "DepartmentManagerId",
                table: "Departmentes");

            migrationBuilder.AddColumn<bool>(
                name: "isManager",
                table: "Engineers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Engineers_Departmentes_DepartmentId",
                table: "Engineers",
                column: "DepartmentId",
                principalTable: "Departmentes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Engineers_Departmentes_DepartmentId",
                table: "Engineers");

            migrationBuilder.DropColumn(
                name: "isManager",
                table: "Engineers");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentManagerId",
                table: "Departmentes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departmentes_DepartmentManagerId",
                table: "Departmentes",
                column: "DepartmentManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departmentes_Engineers_DepartmentManagerId",
                table: "Departmentes",
                column: "DepartmentManagerId",
                principalTable: "Engineers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Engineers_Departmentes_DepartmentId",
                table: "Engineers",
                column: "DepartmentId",
                principalTable: "Departmentes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
