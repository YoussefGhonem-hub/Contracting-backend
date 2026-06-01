using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMessageTypeAndAttachmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProgressPercent",
                schema: "master",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageType",
                schema: "client",
                table: "ChatMessages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Text");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentType",
                schema: "client",
                table: "ChatMessageAttachments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Document");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgressPercent",
                schema: "master",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "MessageType",
                schema: "client",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "AttachmentType",
                schema: "client",
                table: "ChatMessageAttachments");
        }
    }
}
