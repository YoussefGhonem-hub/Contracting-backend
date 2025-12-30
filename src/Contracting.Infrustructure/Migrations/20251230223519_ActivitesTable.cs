using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class ActivitesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EngineerRequestNotes_Statuses_StatusId",
                table: "EngineerRequestNotes");

            migrationBuilder.DropIndex(
                name: "IX_EngineerRequestNotes_StatusId",
                table: "EngineerRequestNotes");

            migrationBuilder.DropColumn(
                name: "isManager",
                table: "Engineers");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "EngineerRequestNotes");

            migrationBuilder.CreateTable(
                name: "EngineerRequestActivites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EngineerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_EngineerRequestActivites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerRequestActivites_EngineerRequests_EngineerRequestId",
                        column: x => x.EngineerRequestId,
                        principalTable: "EngineerRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EngineerRequestActivites_Engineers_EngineerId",
                        column: x => x.EngineerId,
                        principalTable: "Engineers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EngineerRequestActivites_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestActivites_EngineerId",
                table: "EngineerRequestActivites",
                column: "EngineerId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestActivites_EngineerRequestId",
                table: "EngineerRequestActivites",
                column: "EngineerRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestActivites_IsDeleted",
                table: "EngineerRequestActivites",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestActivites_StatusId",
                table: "EngineerRequestActivites",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerRequestActivites");

            migrationBuilder.AddColumn<bool>(
                name: "isManager",
                table: "Engineers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "EngineerRequestNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestNotes_StatusId",
                table: "EngineerRequestNotes",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_EngineerRequestNotes_Statuses_StatusId",
                table: "EngineerRequestNotes",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id");
        }
    }
}
