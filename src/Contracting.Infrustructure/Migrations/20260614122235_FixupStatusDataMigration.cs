using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class FixupStatusDataMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Resolve the 4 Status IDs by Code so this works in every environment.
            // Using a CTE to avoid repeating the sub-select per table.

            // TransferRequests: any row whose StatusId is still NULL gets mapped to New.
            // (All non-NULL rows were already migrated by UnifyRequestStatusToMasterTable.)
            migrationBuilder.Sql(@"
                UPDATE [business].[TransferRequests]
                SET [StatusId] = (SELECT [Id] FROM [master].[Statuses] WHERE [Code] = 'NEW')
                WHERE [StatusId] IS NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE [business].[TransferRequestActivities]
                SET [ToStatusId] = (SELECT [Id] FROM [master].[Statuses] WHERE [Code] = 'NEW')
                WHERE [ToStatusId] IS NULL OR [ToStatusId] = '00000000-0000-0000-0000-000000000000'
            ");

            // LaborAttendanceRequests
            migrationBuilder.Sql(@"
                UPDATE [business].[LaborAttendanceRequests]
                SET [StatusId] = (SELECT [Id] FROM [master].[Statuses] WHERE [Code] = 'NEW')
                WHERE [StatusId] IS NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE [business].[LaborAttendanceActivities]
                SET [ToStatusId] = (SELECT [Id] FROM [master].[Statuses] WHERE [Code] = 'NEW')
                WHERE [ToStatusId] IS NULL OR [ToStatusId] = '00000000-0000-0000-0000-000000000000'
            ");

            // FinancialClearances
            migrationBuilder.Sql(@"
                UPDATE [business].[FinancialClearances]
                SET [StatusId] = (SELECT [Id] FROM [master].[Statuses] WHERE [Code] = 'NEW')
                WHERE [StatusId] IS NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE [business].[FinancialClearanceActivities]
                SET [ToStatusId] = (SELECT [Id] FROM [master].[Statuses] WHERE [Code] = 'NEW')
                WHERE [ToStatusId] IS NULL OR [ToStatusId] = '00000000-0000-0000-0000-000000000000'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback needed — fixing NULL values to New is safe to keep.
        }
    }
}
