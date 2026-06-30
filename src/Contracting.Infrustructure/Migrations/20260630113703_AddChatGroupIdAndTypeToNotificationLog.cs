using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatGroupIdAndTypeToNotificationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChatGroupId",
                schema: "helper",
                table: "NotificationLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                schema: "helper",
                table: "NotificationLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_NotificationLogs_ChatGroupId",
                schema: "helper",
                table: "NotificationLogs",
                column: "ChatGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_NotificationLogs_ChatGroupId",
                schema: "helper",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "ChatGroupId",
                schema: "helper",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "helper",
                table: "NotificationLogs");
        }
    }
}
