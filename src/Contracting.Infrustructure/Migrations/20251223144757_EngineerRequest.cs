using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class EngineerRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EngineerRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PriorityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EngineerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Descreption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_EngineerRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerRequests_Departmentes_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departmentes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EngineerRequests_Engineers_EngineerId",
                        column: x => x.EngineerId,
                        principalTable: "Engineers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EngineerRequests_Priorities_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "Priorities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EngineerRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequests_DepartmentId",
                table: "EngineerRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequests_EngineerId",
                table: "EngineerRequests",
                column: "EngineerId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequests_IsDeleted",
                table: "EngineerRequests",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequests_PriorityId",
                table: "EngineerRequests",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequests_ProjectId",
                table: "EngineerRequests",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerRequests");
        }
    }
}
