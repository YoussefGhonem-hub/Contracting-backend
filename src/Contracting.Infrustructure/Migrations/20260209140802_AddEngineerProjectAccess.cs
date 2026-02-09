using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineerProjectAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EngineerProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_EngineerProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerProjects_Engineers_EngineerId",
                        column: x => x.EngineerId,
                        principalSchema: "master",
                        principalTable: "Engineers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EngineerProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "master",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerProjects_EngineerId",
                table: "EngineerProjects",
                column: "EngineerId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerProjects_IsDeleted",
                table: "EngineerProjects",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerProjects_ProjectId",
                table: "EngineerProjects",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerProjects");
        }
    }
}
