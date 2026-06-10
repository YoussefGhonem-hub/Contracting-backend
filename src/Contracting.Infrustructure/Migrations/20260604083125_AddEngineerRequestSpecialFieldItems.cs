using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineerRequestSpecialFieldItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EngineerRequestSpecialFieldItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentSpecialFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConstructionItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EngineerRequestSpecialFieldItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerRequestSpecialFieldItems_ConstructionItems_ConstructionItemId",
                        column: x => x.ConstructionItemId,
                        principalSchema: "master",
                        principalTable: "ConstructionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EngineerRequestSpecialFieldItems_DepartmentSpecialFields_DepartmentSpecialFieldId",
                        column: x => x.DepartmentSpecialFieldId,
                        principalSchema: "master",
                        principalTable: "DepartmentSpecialFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EngineerRequestSpecialFieldItems_EngineerRequests_EngineerRequestId",
                        column: x => x.EngineerRequestId,
                        principalSchema: "business",
                        principalTable: "EngineerRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestSpecialFieldItems_ConstructionItemId",
                table: "EngineerRequestSpecialFieldItems",
                column: "ConstructionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestSpecialFieldItems_DepartmentSpecialFieldId",
                table: "EngineerRequestSpecialFieldItems",
                column: "DepartmentSpecialFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestSpecialFieldItems_EngineerRequestId",
                table: "EngineerRequestSpecialFieldItems",
                column: "EngineerRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerRequestSpecialFieldItems_IsDeleted",
                table: "EngineerRequestSpecialFieldItems",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerRequestSpecialFieldItems");
        }
    }
}
