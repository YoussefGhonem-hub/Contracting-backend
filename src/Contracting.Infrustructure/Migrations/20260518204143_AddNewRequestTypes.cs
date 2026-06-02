using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewRequestTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialClearances",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClearanceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmployeeName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdvanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SpentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_FinancialClearances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialClearances_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "master",
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialClearances_Engineers_RequestedById",
                        column: x => x.RequestedById,
                        principalSchema: "master",
                        principalTable: "Engineers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialClearances_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "master",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LaborAttendanceRequests",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SiteName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_LaborAttendanceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaborAttendanceRequests_Engineers_SupervisorId",
                        column: x => x.SupervisorId,
                        principalSchema: "master",
                        principalTable: "Engineers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LaborAttendanceRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "master",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransferRequests",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceWarehouse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DestinationProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DestinationWarehouse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_TransferRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferRequests_Engineers_RequestedById",
                        column: x => x.RequestedById,
                        principalSchema: "master",
                        principalTable: "Engineers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferRequests_Projects_DestinationProjectId",
                        column: x => x.DestinationProjectId,
                        principalSchema: "master",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferRequests_Projects_SourceProjectId",
                        column: x => x.SourceProjectId,
                        principalSchema: "master",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialClearanceActivities",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinancialClearanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_FinancialClearanceActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialClearanceActivities_Engineers_EngineerId",
                        column: x => x.EngineerId,
                        principalSchema: "master",
                        principalTable: "Engineers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialClearanceActivities_FinancialClearances_FinancialClearanceId",
                        column: x => x.FinancialClearanceId,
                        principalSchema: "business",
                        principalTable: "FinancialClearances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialClearanceAttachments",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinancialClearanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AttachmentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_FinancialClearanceAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialClearanceAttachments_FinancialClearances_FinancialClearanceId",
                        column: x => x.FinancialClearanceId,
                        principalSchema: "business",
                        principalTable: "FinancialClearances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaborAttendanceActivities",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaborAttendanceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_LaborAttendanceActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaborAttendanceActivities_Engineers_EngineerId",
                        column: x => x.EngineerId,
                        principalSchema: "master",
                        principalTable: "Engineers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LaborAttendanceActivities_LaborAttendanceRequests_LaborAttendanceRequestId",
                        column: x => x.LaborAttendanceRequestId,
                        principalSchema: "business",
                        principalTable: "LaborAttendanceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaborAttendanceAttachments",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaborAttendanceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_LaborAttendanceAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaborAttendanceAttachments_LaborAttendanceRequests_LaborAttendanceRequestId",
                        column: x => x.LaborAttendanceRequestId,
                        principalSchema: "business",
                        principalTable: "LaborAttendanceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaborAttendanceRecords",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaborAttendanceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AttendanceStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DailyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_LaborAttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaborAttendanceRecords_LaborAttendanceRequests_LaborAttendanceRequestId",
                        column: x => x.LaborAttendanceRequestId,
                        principalSchema: "business",
                        principalTable: "LaborAttendanceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferRequestActivities",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EngineerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_TransferRequestActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferRequestActivities_Engineers_EngineerId",
                        column: x => x.EngineerId,
                        principalSchema: "master",
                        principalTable: "Engineers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransferRequestActivities_TransferRequests_TransferRequestId",
                        column: x => x.TransferRequestId,
                        principalSchema: "business",
                        principalTable: "TransferRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferRequestAttachments",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_TransferRequestAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferRequestAttachments_TransferRequests_TransferRequestId",
                        column: x => x.TransferRequestId,
                        principalSchema: "business",
                        principalTable: "TransferRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferRequestItems",
                schema: "business",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_TransferRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferRequestItems_TransferRequests_TransferRequestId",
                        column: x => x.TransferRequestId,
                        principalSchema: "business",
                        principalTable: "TransferRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearanceActivities_EngineerId",
                schema: "business",
                table: "FinancialClearanceActivities",
                column: "EngineerId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearanceActivities_FinancialClearanceId",
                schema: "business",
                table: "FinancialClearanceActivities",
                column: "FinancialClearanceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearanceActivities_IsDeleted",
                schema: "business",
                table: "FinancialClearanceActivities",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearanceAttachments_FinancialClearanceId",
                schema: "business",
                table: "FinancialClearanceAttachments",
                column: "FinancialClearanceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearanceAttachments_IsDeleted",
                schema: "business",
                table: "FinancialClearanceAttachments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearances_DepartmentId",
                schema: "business",
                table: "FinancialClearances",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearances_IsDeleted",
                schema: "business",
                table: "FinancialClearances",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearances_ProjectId",
                schema: "business",
                table: "FinancialClearances",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearances_RequestedById",
                schema: "business",
                table: "FinancialClearances",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceActivities_EngineerId",
                schema: "business",
                table: "LaborAttendanceActivities",
                column: "EngineerId");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceActivities_IsDeleted",
                schema: "business",
                table: "LaborAttendanceActivities",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceActivities_LaborAttendanceRequestId",
                schema: "business",
                table: "LaborAttendanceActivities",
                column: "LaborAttendanceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceAttachments_IsDeleted",
                schema: "business",
                table: "LaborAttendanceAttachments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceAttachments_LaborAttendanceRequestId",
                schema: "business",
                table: "LaborAttendanceAttachments",
                column: "LaborAttendanceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceRecords_IsDeleted",
                schema: "business",
                table: "LaborAttendanceRecords",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceRecords_LaborAttendanceRequestId",
                schema: "business",
                table: "LaborAttendanceRecords",
                column: "LaborAttendanceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceRequests_IsDeleted",
                schema: "business",
                table: "LaborAttendanceRequests",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceRequests_ProjectId",
                schema: "business",
                table: "LaborAttendanceRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceRequests_SupervisorId",
                schema: "business",
                table: "LaborAttendanceRequests",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequestActivities_EngineerId",
                schema: "business",
                table: "TransferRequestActivities",
                column: "EngineerId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequestActivities_IsDeleted",
                schema: "business",
                table: "TransferRequestActivities",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequestActivities_TransferRequestId",
                schema: "business",
                table: "TransferRequestActivities",
                column: "TransferRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequestAttachments_IsDeleted",
                schema: "business",
                table: "TransferRequestAttachments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequestAttachments_TransferRequestId",
                schema: "business",
                table: "TransferRequestAttachments",
                column: "TransferRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequestItems_IsDeleted",
                schema: "business",
                table: "TransferRequestItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequestItems_TransferRequestId",
                schema: "business",
                table: "TransferRequestItems",
                column: "TransferRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_DestinationProjectId",
                schema: "business",
                table: "TransferRequests",
                column: "DestinationProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_IsDeleted",
                schema: "business",
                table: "TransferRequests",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_RequestedById",
                schema: "business",
                table: "TransferRequests",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_SourceProjectId",
                schema: "business",
                table: "TransferRequests",
                column: "SourceProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialClearanceActivities",
                schema: "business");

            migrationBuilder.DropTable(
                name: "FinancialClearanceAttachments",
                schema: "business");

            migrationBuilder.DropTable(
                name: "LaborAttendanceActivities",
                schema: "business");

            migrationBuilder.DropTable(
                name: "LaborAttendanceAttachments",
                schema: "business");

            migrationBuilder.DropTable(
                name: "LaborAttendanceRecords",
                schema: "business");

            migrationBuilder.DropTable(
                name: "TransferRequestActivities",
                schema: "business");

            migrationBuilder.DropTable(
                name: "TransferRequestAttachments",
                schema: "business");

            migrationBuilder.DropTable(
                name: "TransferRequestItems",
                schema: "business");

            migrationBuilder.DropTable(
                name: "FinancialClearances",
                schema: "business");

            migrationBuilder.DropTable(
                name: "LaborAttendanceRequests",
                schema: "business");

            migrationBuilder.DropTable(
                name: "TransferRequests",
                schema: "business");
        }
    }
}
