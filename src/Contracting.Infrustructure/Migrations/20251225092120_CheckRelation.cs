using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class CheckRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "EngineerRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "assignToId",
                table: "EngineerRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequests_assignToId",
                table: "EngineerRequests",
                column: "assignToId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequests_StatusId",
                table: "EngineerRequests",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_EngineerRequests_Engineers_assignToId",
                table: "EngineerRequests",
                column: "assignToId",
                principalTable: "Engineers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EngineerRequests_Statuses_StatusId",
                table: "EngineerRequests",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EngineerRequests_Engineers_assignToId",
                table: "EngineerRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_EngineerRequests_Statuses_StatusId",
                table: "EngineerRequests");

            migrationBuilder.DropIndex(
                name: "IX_EngineerRequests_assignToId",
                table: "EngineerRequests");

            migrationBuilder.DropIndex(
                name: "IX_EngineerRequests_StatusId",
                table: "EngineerRequests");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "EngineerRequests");

            migrationBuilder.DropColumn(
                name: "assignToId",
                table: "EngineerRequests");
        }
    }
}
