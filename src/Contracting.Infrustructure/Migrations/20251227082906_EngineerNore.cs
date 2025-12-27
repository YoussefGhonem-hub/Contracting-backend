using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class EngineerNore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "timeDuration",
                table: "EngineerRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EngineerRequestNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineerRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EngineerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_EngineerRequestNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerRequestNotes_EngineerRequests_EngineerRequestId",
                        column: x => x.EngineerRequestId,
                        principalTable: "EngineerRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EngineerRequestNotes_Engineers_EngineerId",
                        column: x => x.EngineerId,
                        principalTable: "Engineers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EngineerRequestNotes_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestNotes_EngineerId",
                table: "EngineerRequestNotes",
                column: "EngineerId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestNotes_EngineerRequestId",
                table: "EngineerRequestNotes",
                column: "EngineerRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestNotes_IsDeleted",
                table: "EngineerRequestNotes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestNotes_StatusId",
                table: "EngineerRequestNotes",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerRequestNotes");

            migrationBuilder.DropColumn(
                name: "timeDuration",
                table: "EngineerRequests");
        }
    }
}
