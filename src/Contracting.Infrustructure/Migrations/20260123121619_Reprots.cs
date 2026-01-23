using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class Reprots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "PasswordResetCodes",
                newName: "PasswordResetCodes",
                newSchema: "security");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "security",
                table: "PasswordResetCodes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "EngineerSiteReports",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReportDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GeneralNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SiteSafetyObservations = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    QualityControlObservations = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_EngineerSiteReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerSiteReports_Engineers_EngineerId",
                        column: x => x.EngineerId,
                        principalSchema: "master",
                        principalTable: "Engineers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EngineerSiteReports_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "master",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EngineerSiteSurveyQuestionTemplates",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    order = table.Column<int>(type: "int", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_EngineerSiteSurveyQuestionTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EngineerSiteEquipments",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerSiteReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    hoursUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    condition = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    isOperational = table.Column<bool>(type: "bit", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_EngineerSiteEquipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerSiteEquipments_EngineerSiteReports_EngineerSiteReportId",
                        column: x => x.EngineerSiteReportId,
                        principalSchema: "business",
                        principalTable: "EngineerSiteReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EngineerSiteMaterials",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerSiteReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    usage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    needMore = table.Column<bool>(type: "bit", nullable: true),
                    unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    unitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    totalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_EngineerSiteMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerSiteMaterials_EngineerSiteReports_EngineerSiteReportId",
                        column: x => x.EngineerSiteReportId,
                        principalSchema: "business",
                        principalTable: "EngineerSiteReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EngineerSiteSurveyQuestions",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerSiteReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    question = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    answer = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_EngineerSiteSurveyQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerSiteSurveyQuestions_EngineerSiteReports_EngineerSiteReportId",
                        column: x => x.EngineerSiteReportId,
                        principalSchema: "business",
                        principalTable: "EngineerSiteReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EngineerSiteWorkLogs",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerSiteReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    totalHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    totalHoursToDate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
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
                    table.PrimaryKey("PK_EngineerSiteWorkLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerSiteWorkLogs_EngineerSiteReports_EngineerSiteReportId",
                        column: x => x.EngineerSiteReportId,
                        principalSchema: "business",
                        principalTable: "EngineerSiteReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EngineerSiteWorkLogAttachments",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EngineerSiteWorkLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_EngineerSiteWorkLogAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngineerSiteWorkLogAttachments_EngineerSiteWorkLogs_EngineerSiteWorkLogId",
                        column: x => x.EngineerSiteWorkLogId,
                        principalSchema: "business",
                        principalTable: "EngineerSiteWorkLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteEquipments_EngineerSiteReportId",
                schema: "business",
                table: "EngineerSiteEquipments",
                column: "EngineerSiteReportId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteEquipments_IsDeleted",
                schema: "business",
                table: "EngineerSiteEquipments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteMaterials_EngineerSiteReportId",
                schema: "business",
                table: "EngineerSiteMaterials",
                column: "EngineerSiteReportId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteMaterials_IsDeleted",
                schema: "business",
                table: "EngineerSiteMaterials",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteReports_EngineerId",
                schema: "business",
                table: "EngineerSiteReports",
                column: "EngineerId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteReports_IsDeleted",
                schema: "business",
                table: "EngineerSiteReports",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteReports_ProjectId",
                schema: "business",
                table: "EngineerSiteReports",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteReports_ReportDate",
                schema: "business",
                table: "EngineerSiteReports",
                column: "ReportDate");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteSurveyQuestions_EngineerSiteReportId",
                schema: "business",
                table: "EngineerSiteSurveyQuestions",
                column: "EngineerSiteReportId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteSurveyQuestions_IsDeleted",
                schema: "business",
                table: "EngineerSiteSurveyQuestions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteSurveyQuestionTemplates_isActive",
                schema: "business",
                table: "EngineerSiteSurveyQuestionTemplates",
                column: "isActive");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteSurveyQuestionTemplates_IsDeleted",
                schema: "business",
                table: "EngineerSiteSurveyQuestionTemplates",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteSurveyQuestionTemplates_order",
                schema: "business",
                table: "EngineerSiteSurveyQuestionTemplates",
                column: "order");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteWorkLogAttachments_EngineerSiteWorkLogId",
                schema: "business",
                table: "EngineerSiteWorkLogAttachments",
                column: "EngineerSiteWorkLogId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteWorkLogAttachments_IsDeleted",
                schema: "business",
                table: "EngineerSiteWorkLogAttachments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteWorkLogs_EngineerSiteReportId",
                schema: "business",
                table: "EngineerSiteWorkLogs",
                column: "EngineerSiteReportId");

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteWorkLogs_IsDeleted",
                schema: "business",
                table: "EngineerSiteWorkLogs",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EngineerSiteEquipments",
                schema: "business");

            migrationBuilder.DropTable(
                name: "EngineerSiteMaterials",
                schema: "business");

            migrationBuilder.DropTable(
                name: "EngineerSiteSurveyQuestions",
                schema: "business");

            migrationBuilder.DropTable(
                name: "EngineerSiteSurveyQuestionTemplates",
                schema: "business");

            migrationBuilder.DropTable(
                name: "EngineerSiteWorkLogAttachments",
                schema: "business");

            migrationBuilder.DropTable(
                name: "EngineerSiteWorkLogs",
                schema: "business");

            migrationBuilder.DropTable(
                name: "EngineerSiteReports",
                schema: "business");

            migrationBuilder.RenameTable(
                name: "PasswordResetCodes",
                schema: "security",
                newName: "PasswordResetCodes");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "PasswordResetCodes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
