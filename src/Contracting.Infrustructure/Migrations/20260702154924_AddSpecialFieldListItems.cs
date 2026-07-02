using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialFieldListItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsList",
                schema: "master",
                table: "DepartmentSpecialFields",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EngineerRequestSpecialFieldListItems",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentSpecialFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_EngineerRequestSpecialFieldListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerRequestSpecialFieldListItems_DepartmentSpecialFields_DepartmentSpecialFieldId",
                        column: x => x.DepartmentSpecialFieldId,
                        principalSchema: "master",
                        principalTable: "DepartmentSpecialFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EngineerRequestSpecialFieldListItems_EngineerRequests_EngineerRequestId",
                        column: x => x.EngineerRequestId,
                        principalSchema: "business",
                        principalTable: "EngineerRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestSpecialFieldListItems_DepartmentSpecialFieldId",
                schema: "business",
                table: "EngineerRequestSpecialFieldListItems",
                column: "DepartmentSpecialFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestSpecialFieldListItems_EngineerRequestId",
                schema: "business",
                table: "EngineerRequestSpecialFieldListItems",
                column: "EngineerRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestSpecialFieldListItems_IsDeleted",
                schema: "business",
                table: "EngineerRequestSpecialFieldListItems",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerRequestSpecialFieldListItems",
                schema: "business");

            migrationBuilder.DropColumn(
                name: "IsList",
                schema: "master",
                table: "DepartmentSpecialFields");
        }
    }
}
