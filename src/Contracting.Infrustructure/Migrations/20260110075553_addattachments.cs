using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class addattachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EngineerRequestAttachments",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EngineerRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EngineerRequestNotesId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineerRequestAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerRequestAttachments_EngineerRequestNotes_EngineerRequestNotesId",
                        column: x => x.EngineerRequestNotesId,
                        principalSchema: "business",
                        principalTable: "EngineerRequestNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EngineerRequestAttachments_EngineerRequests_EngineerRequestId",
                        column: x => x.EngineerRequestId,
                        principalSchema: "business",
                        principalTable: "EngineerRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestAttachments_EngineerRequestId",
                schema: "business",
                table: "EngineerRequestAttachments",
                column: "EngineerRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestAttachments_EngineerRequestNotesId",
                schema: "business",
                table: "EngineerRequestAttachments",
                column: "EngineerRequestNotesId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestAttachments_IsDeleted",
                schema: "business",
                table: "EngineerRequestAttachments",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerRequestAttachments",
                schema: "business");
        }
    }
}
