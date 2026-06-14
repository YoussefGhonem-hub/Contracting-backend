using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConstructionItemUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                schema: "master",
                table: "ConstructionItems");

            migrationBuilder.CreateTable(
                name: "ConstructionItemUnits",
                schema: "master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    nameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ConstructionItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_ConstructionItemUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConstructionItemUnits_ConstructionItems_ConstructionItemId",
                        column: x => x.ConstructionItemId,
                        principalSchema: "master",
                        principalTable: "ConstructionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionItemUnits_ConstructionItemId",
                schema: "master",
                table: "ConstructionItemUnits",
                column: "ConstructionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionItemUnits_IsDeleted",
                schema: "master",
                table: "ConstructionItemUnits",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConstructionItemUnits",
                schema: "master");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                schema: "master",
                table: "ConstructionItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
