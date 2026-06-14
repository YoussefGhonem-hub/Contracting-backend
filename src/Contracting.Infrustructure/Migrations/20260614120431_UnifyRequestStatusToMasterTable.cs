using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class UnifyRequestStatusToMasterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add new nullable StatusId / FromStatusId / ToStatusId columns
            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                schema: "business",
                table: "TransferRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FromStatusId",
                schema: "business",
                table: "TransferRequestActivities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ToStatusId",
                schema: "business",
                table: "TransferRequestActivities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                schema: "business",
                table: "LaborAttendanceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FromStatusId",
                schema: "business",
                table: "LaborAttendanceActivities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ToStatusId",
                schema: "business",
                table: "LaborAttendanceActivities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                schema: "business",
                table: "FinancialClearances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FromStatusId",
                schema: "business",
                table: "FinancialClearanceActivities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ToStatusId",
                schema: "business",
                table: "FinancialClearanceActivities",
                type: "uniqueidentifier",
                nullable: true);

            // Step 2: Migrate data from old string enum columns to new Status Guid FKs
            // Status IDs from master.Statuses:
            //   New        = 31CBE1DB-1813-4E45-AAEC-5F63A3974654
            //   InProgress = D3F0DDDF-C367-4030-8EF5-638774DCE5B2
            //   Completed  = BEC2B30D-9312-41AF-B92C-6F9438A70931
            //   Rejected   = 8F36EE76-E80B-4451-BF2B-E2873F9DE772

            // TransferRequests: PendingReceipt→New, Draft/PartiallyReceived→InProgress, Closed→Completed, Cancelled→Rejected
            migrationBuilder.Sql(@"
                UPDATE [business].[TransferRequests] SET [StatusId] =
                    CASE [Status]
                        WHEN 'PendingReceipt'      THEN '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                        WHEN 'Draft'               THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'PartiallyReceived'   THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'Closed'              THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Cancelled'           THEN '8F36EE76-E80B-4451-BF2B-E2873F9DE772'
                        ELSE '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                    END
            ");

            migrationBuilder.Sql(@"
                UPDATE [business].[TransferRequestActivities] SET
                    [FromStatusId] = CASE [FromStatus]
                        WHEN 'PendingReceipt'      THEN '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                        WHEN 'Draft'               THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'PartiallyReceived'   THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'Closed'              THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Cancelled'           THEN '8F36EE76-E80B-4451-BF2B-E2873F9DE772'
                        ELSE NULL
                    END,
                    [ToStatusId] = CASE [ToStatus]
                        WHEN 'PendingReceipt'      THEN '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                        WHEN 'Draft'               THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'PartiallyReceived'   THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'Closed'              THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Cancelled'           THEN '8F36EE76-E80B-4451-BF2B-E2873F9DE772'
                        ELSE '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                    END
            ");

            // LaborAttendanceRequests: Pending→New, Assigned→InProgress, Validated→Completed, Rejected→Rejected
            migrationBuilder.Sql(@"
                UPDATE [business].[LaborAttendanceRequests] SET [StatusId] =
                    CASE [Status]
                        WHEN 'Pending'    THEN '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                        WHEN 'Assigned'   THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'Validated'  THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Rejected'   THEN '8F36EE76-E80B-4451-BF2B-E2873F9DE772'
                        ELSE '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                    END
            ");

            migrationBuilder.Sql(@"
                UPDATE [business].[LaborAttendanceActivities] SET
                    [FromStatusId] = CASE [FromStatus]
                        WHEN 'Pending'    THEN '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                        WHEN 'Assigned'   THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'Validated'  THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Rejected'   THEN '8F36EE76-E80B-4451-BF2B-E2873F9DE772'
                        ELSE NULL
                    END,
                    [ToStatusId] = CASE [ToStatus]
                        WHEN 'Pending'    THEN '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                        WHEN 'Assigned'   THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'Validated'  THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Rejected'   THEN '8F36EE76-E80B-4451-BF2B-E2873F9DE772'
                        ELSE '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                    END
            ");

            // FinancialClearances: Submitted→New, Draft/UnderReview→InProgress, Approved/Closed→Completed, Rejected→Rejected
            migrationBuilder.Sql(@"
                UPDATE [business].[FinancialClearances] SET [StatusId] =
                    CASE [Status]
                        WHEN 'Submitted'    THEN '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                        WHEN 'Draft'        THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'UnderReview'  THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'Approved'     THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Closed'       THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Rejected'     THEN '8F36EE76-E80B-4451-BF2B-E2873F9DE772'
                        ELSE '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                    END
            ");

            migrationBuilder.Sql(@"
                UPDATE [business].[FinancialClearanceActivities] SET
                    [FromStatusId] = CASE [FromStatus]
                        WHEN 'Submitted'    THEN '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                        WHEN 'Draft'        THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'UnderReview'  THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'Approved'     THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Closed'       THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Rejected'     THEN '8F36EE76-E80B-4451-BF2B-E2873F9DE772'
                        ELSE NULL
                    END,
                    [ToStatusId] = CASE [ToStatus]
                        WHEN 'Submitted'    THEN '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                        WHEN 'Draft'        THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'UnderReview'  THEN 'D3F0DDDF-C367-4030-8EF5-638774DCE5B2'
                        WHEN 'Approved'     THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Closed'       THEN 'BEC2B30D-9312-41AF-B92C-6F9438A70931'
                        WHEN 'Rejected'     THEN '8F36EE76-E80B-4451-BF2B-E2873F9DE772'
                        ELSE '31CBE1DB-1813-4E45-AAEC-5F63A3974654'
                    END
            ");

            // Step 3: Drop old string enum columns
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "business",
                table: "TransferRequests");

            migrationBuilder.DropColumn(
                name: "FromStatus",
                schema: "business",
                table: "TransferRequestActivities");

            migrationBuilder.DropColumn(
                name: "ToStatus",
                schema: "business",
                table: "TransferRequestActivities");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "business",
                table: "LaborAttendanceRequests");

            migrationBuilder.DropColumn(
                name: "FromStatus",
                schema: "business",
                table: "LaborAttendanceActivities");

            migrationBuilder.DropColumn(
                name: "ToStatus",
                schema: "business",
                table: "LaborAttendanceActivities");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "business",
                table: "FinancialClearances");

            migrationBuilder.DropColumn(
                name: "FromStatus",
                schema: "business",
                table: "FinancialClearanceActivities");

            migrationBuilder.DropColumn(
                name: "ToStatus",
                schema: "business",
                table: "FinancialClearanceActivities");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_StatusId",
                schema: "business",
                table: "TransferRequests",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequestActivities_FromStatusId",
                schema: "business",
                table: "TransferRequestActivities",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequestActivities_ToStatusId",
                schema: "business",
                table: "TransferRequestActivities",
                column: "ToStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceRequests_StatusId",
                schema: "business",
                table: "LaborAttendanceRequests",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceActivities_FromStatusId",
                schema: "business",
                table: "LaborAttendanceActivities",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LaborAttendanceActivities_ToStatusId",
                schema: "business",
                table: "LaborAttendanceActivities",
                column: "ToStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearances_StatusId",
                schema: "business",
                table: "FinancialClearances",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearanceActivities_FromStatusId",
                schema: "business",
                table: "FinancialClearanceActivities",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClearanceActivities_ToStatusId",
                schema: "business",
                table: "FinancialClearanceActivities",
                column: "ToStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialClearanceActivities_Statuses_FromStatusId",
                schema: "business",
                table: "FinancialClearanceActivities",
                column: "FromStatusId",
                principalSchema: "master",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialClearanceActivities_Statuses_ToStatusId",
                schema: "business",
                table: "FinancialClearanceActivities",
                column: "ToStatusId",
                principalSchema: "master",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialClearances_Statuses_StatusId",
                schema: "business",
                table: "FinancialClearances",
                column: "StatusId",
                principalSchema: "master",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LaborAttendanceActivities_Statuses_FromStatusId",
                schema: "business",
                table: "LaborAttendanceActivities",
                column: "FromStatusId",
                principalSchema: "master",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LaborAttendanceActivities_Statuses_ToStatusId",
                schema: "business",
                table: "LaborAttendanceActivities",
                column: "ToStatusId",
                principalSchema: "master",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LaborAttendanceRequests_Statuses_StatusId",
                schema: "business",
                table: "LaborAttendanceRequests",
                column: "StatusId",
                principalSchema: "master",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransferRequestActivities_Statuses_FromStatusId",
                schema: "business",
                table: "TransferRequestActivities",
                column: "FromStatusId",
                principalSchema: "master",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransferRequestActivities_Statuses_ToStatusId",
                schema: "business",
                table: "TransferRequestActivities",
                column: "ToStatusId",
                principalSchema: "master",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransferRequests_Statuses_StatusId",
                schema: "business",
                table: "TransferRequests",
                column: "StatusId",
                principalSchema: "master",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialClearanceActivities_Statuses_FromStatusId",
                schema: "business",
                table: "FinancialClearanceActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialClearanceActivities_Statuses_ToStatusId",
                schema: "business",
                table: "FinancialClearanceActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialClearances_Statuses_StatusId",
                schema: "business",
                table: "FinancialClearances");

            migrationBuilder.DropForeignKey(
                name: "FK_LaborAttendanceActivities_Statuses_FromStatusId",
                schema: "business",
                table: "LaborAttendanceActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_LaborAttendanceActivities_Statuses_ToStatusId",
                schema: "business",
                table: "LaborAttendanceActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_LaborAttendanceRequests_Statuses_StatusId",
                schema: "business",
                table: "LaborAttendanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferRequestActivities_Statuses_FromStatusId",
                schema: "business",
                table: "TransferRequestActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferRequestActivities_Statuses_ToStatusId",
                schema: "business",
                table: "TransferRequestActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferRequests_Statuses_StatusId",
                schema: "business",
                table: "TransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_TransferRequests_StatusId",
                schema: "business",
                table: "TransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_TransferRequestActivities_FromStatusId",
                schema: "business",
                table: "TransferRequestActivities");

            migrationBuilder.DropIndex(
                name: "IX_TransferRequestActivities_ToStatusId",
                schema: "business",
                table: "TransferRequestActivities");

            migrationBuilder.DropIndex(
                name: "IX_LaborAttendanceRequests_StatusId",
                schema: "business",
                table: "LaborAttendanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_LaborAttendanceActivities_FromStatusId",
                schema: "business",
                table: "LaborAttendanceActivities");

            migrationBuilder.DropIndex(
                name: "IX_LaborAttendanceActivities_ToStatusId",
                schema: "business",
                table: "LaborAttendanceActivities");

            migrationBuilder.DropIndex(
                name: "IX_FinancialClearances_StatusId",
                schema: "business",
                table: "FinancialClearances");

            migrationBuilder.DropIndex(
                name: "IX_FinancialClearanceActivities_FromStatusId",
                schema: "business",
                table: "FinancialClearanceActivities");

            migrationBuilder.DropIndex(
                name: "IX_FinancialClearanceActivities_ToStatusId",
                schema: "business",
                table: "FinancialClearanceActivities");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "business",
                table: "TransferRequests");

            migrationBuilder.DropColumn(
                name: "FromStatusId",
                schema: "business",
                table: "TransferRequestActivities");

            migrationBuilder.DropColumn(
                name: "ToStatusId",
                schema: "business",
                table: "TransferRequestActivities");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "business",
                table: "LaborAttendanceRequests");

            migrationBuilder.DropColumn(
                name: "FromStatusId",
                schema: "business",
                table: "LaborAttendanceActivities");

            migrationBuilder.DropColumn(
                name: "ToStatusId",
                schema: "business",
                table: "LaborAttendanceActivities");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "business",
                table: "FinancialClearances");

            migrationBuilder.DropColumn(
                name: "FromStatusId",
                schema: "business",
                table: "FinancialClearanceActivities");

            migrationBuilder.DropColumn(
                name: "ToStatusId",
                schema: "business",
                table: "FinancialClearanceActivities");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "business",
                table: "TransferRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FromStatus",
                schema: "business",
                table: "TransferRequestActivities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToStatus",
                schema: "business",
                table: "TransferRequestActivities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "business",
                table: "LaborAttendanceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FromStatus",
                schema: "business",
                table: "LaborAttendanceActivities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToStatus",
                schema: "business",
                table: "LaborAttendanceActivities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "business",
                table: "FinancialClearances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FromStatus",
                schema: "business",
                table: "FinancialClearanceActivities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToStatus",
                schema: "business",
                table: "FinancialClearanceActivities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
