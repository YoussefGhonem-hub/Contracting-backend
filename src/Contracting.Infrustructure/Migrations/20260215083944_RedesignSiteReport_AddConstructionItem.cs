using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class RedesignSiteReport_AddConstructionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SiteSafetyObservations",
                schema: "business",
                table: "EngineerSiteReports",
                newName: "WorkPerformedToday");

            migrationBuilder.RenameColumn(
                name: "QualityControlObservations",
                schema: "business",
                table: "EngineerSiteReports",
                newName: "VisitDetails");

            migrationBuilder.RenameColumn(
                name: "GeneralNotes",
                schema: "business",
                table: "EngineerSiteReports",
                newName: "MaterialDetails");

            migrationBuilder.AddColumn<bool>(
                name: "ClientVisitToday",
                schema: "business",
                table: "EngineerSiteReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IssuesOrDelays",
                schema: "business",
                table: "EngineerSiteReports",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConstructionItems",
                schema: "master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    nameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_ConstructionItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EngineerSiteReportAttachments",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EngineerSiteReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_EngineerSiteReportAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerSiteReportAttachments_EngineerSiteReports_EngineerSiteReportId",
                        column: x => x.EngineerSiteReportId,
                        principalSchema: "business",
                        principalTable: "EngineerSiteReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportConstructionItemWorkers",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerSiteReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConstructionItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ReportConstructionItemWorkers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportConstructionItemWorkers_ConstructionItems_ConstructionItemId",
                        column: x => x.ConstructionItemId,
                        principalSchema: "master",
                        principalTable: "ConstructionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportConstructionItemWorkers_EngineerSiteReports_EngineerSiteReportId",
                        column: x => x.EngineerSiteReportId,
                        principalSchema: "business",
                        principalTable: "EngineerSiteReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionItems_IsDeleted",
                schema: "master",
                table: "ConstructionItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteReportAttachments_EngineerSiteReportId",
                schema: "business",
                table: "EngineerSiteReportAttachments",
                column: "EngineerSiteReportId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteReportAttachments_IsDeleted",
                schema: "business",
                table: "EngineerSiteReportAttachments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ReportConstructionItemWorkers_ConstructionItemId",
                schema: "business",
                table: "ReportConstructionItemWorkers",
                column: "ConstructionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportConstructionItemWorkers_EngineerSiteReportId",
                schema: "business",
                table: "ReportConstructionItemWorkers",
                column: "EngineerSiteReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportConstructionItemWorkers_IsDeleted",
                schema: "business",
                table: "ReportConstructionItemWorkers",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerSiteReportAttachments",
                schema: "business");

            migrationBuilder.DropTable(
                name: "ReportConstructionItemWorkers",
                schema: "business");

            migrationBuilder.DropTable(
                name: "ConstructionItems",
                schema: "master");

            migrationBuilder.DropColumn(
                name: "ClientVisitToday",
                schema: "business",
                table: "EngineerSiteReports");

            migrationBuilder.DropColumn(
                name: "IssuesOrDelays",
                schema: "business",
                table: "EngineerSiteReports");

            migrationBuilder.RenameColumn(
                name: "WorkPerformedToday",
                schema: "business",
                table: "EngineerSiteReports",
                newName: "SiteSafetyObservations");

            migrationBuilder.RenameColumn(
                name: "VisitDetails",
                schema: "business",
                table: "EngineerSiteReports",
                newName: "QualityControlObservations");

            migrationBuilder.RenameColumn(
                name: "MaterialDetails",
                schema: "business",
                table: "EngineerSiteReports",
                newName: "GeneralNotes");
        }
    }
}
