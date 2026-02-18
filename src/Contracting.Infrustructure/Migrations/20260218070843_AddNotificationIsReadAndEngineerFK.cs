using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationIsReadAndEngineerFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                schema: "helper",
                table: "NotificationLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReadAt",
                schema: "helper",
                table: "NotificationLogs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_NotificationLogs_EngineerId",
                schema: "helper",
                table: "NotificationLogs",
                column: "EngineerId");

            migrationBuilder.CreateIndex(
                name: "idx_NotificationLogs_IsRead",
                schema: "helper",
                table: "NotificationLogs",
                column: "IsRead");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationLogs_Engineers_EngineerId",
                schema: "helper",
                table: "NotificationLogs",
                column: "EngineerId",
                principalSchema: "master",
                principalTable: "Engineers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationLogs_Engineers_EngineerId",
                schema: "helper",
                table: "NotificationLogs");

            migrationBuilder.DropIndex(
                name: "idx_NotificationLogs_EngineerId",
                schema: "helper",
                table: "NotificationLogs");

            migrationBuilder.DropIndex(
                name: "idx_NotificationLogs_IsRead",
                schema: "helper",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "IsRead",
                schema: "helper",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                schema: "helper",
                table: "NotificationLogs");
        }
    }
}
