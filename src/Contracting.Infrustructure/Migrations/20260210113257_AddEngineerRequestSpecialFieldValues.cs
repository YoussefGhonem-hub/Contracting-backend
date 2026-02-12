using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineerRequestSpecialFieldValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EngineerRequestSpecialFieldValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectSpecialFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_EngineerRequestSpecialFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerRequestSpecialFieldValues_EngineerRequests_EngineerRequestId",
                        column: x => x.EngineerRequestId,
                        principalSchema: "business",
                        principalTable: "EngineerRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EngineerRequestSpecialFieldValues_ProjectSpecialFields_ProjectSpecialFieldId",
                        column: x => x.ProjectSpecialFieldId,
                        principalTable: "ProjectSpecialFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestSpecialFieldValues_EngineerRequestId",
                table: "EngineerRequestSpecialFieldValues",
                column: "EngineerRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestSpecialFieldValues_IsDeleted",
                table: "EngineerRequestSpecialFieldValues",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestSpecialFieldValues_ProjectSpecialFieldId",
                table: "EngineerRequestSpecialFieldValues",
                column: "ProjectSpecialFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerRequestSpecialFieldValues");
        }
    }
}
