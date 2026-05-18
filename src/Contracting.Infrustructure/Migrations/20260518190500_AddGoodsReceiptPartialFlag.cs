using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoodsReceiptPartialFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseRequestReceipts",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPartialReceipt = table.Column<bool>(type: "bit", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PurchaseRequestReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestReceipts_EngineerRequests_EngineerRequestId",
                        column: x => x.EngineerRequestId,
                        principalSchema: "business",
                        principalTable: "EngineerRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestReceipts_Engineers_ReceivedById",
                        column: x => x.ReceivedById,
                        principalSchema: "master",
                        principalTable: "Engineers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestReceipts_EngineerRequestId",
                schema: "business",
                table: "PurchaseRequestReceipts",
                column: "EngineerRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestReceipts_IsDeleted",
                schema: "business",
                table: "PurchaseRequestReceipts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestReceipts_ReceivedById",
                schema: "business",
                table: "PurchaseRequestReceipts",
                column: "ReceivedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseRequestReceipts",
                schema: "business");
        }
    }
}
