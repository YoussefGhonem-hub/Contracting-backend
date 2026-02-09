using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectSpecialFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "hasSpecialFields",
                schema: "master",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SpecialFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fieldType = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_SpecialFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSpecialFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecialFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_ProjectSpecialFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectSpecialFields_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "master",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectSpecialFields_SpecialFields_SpecialFieldId",
                        column: x => x.SpecialFieldId,
                        principalTable: "SpecialFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSpecialFields_IsDeleted",
                table: "ProjectSpecialFields",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSpecialFields_ProjectId",
                table: "ProjectSpecialFields",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSpecialFields_SpecialFieldId",
                table: "ProjectSpecialFields",
                column: "SpecialFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialFields_IsDeleted",
                table: "SpecialFields",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectSpecialFields");

            migrationBuilder.DropTable(
                name: "SpecialFields");

            migrationBuilder.DropColumn(
                name: "hasSpecialFields",
                schema: "master",
                table: "Projects");
        }
    }
}
