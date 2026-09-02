using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedOnHoldStatus : Migration
    {
        // Fixed Id so the status resolves to the same row in every environment.
        private const string OnHoldStatusId = "0F7B9C2D-4E6A-4B1C-9D8E-3A5C7B1F2E4D";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Re-introduce the "On Hold" status (it was previously repurposed into "Missing Information"
            // by 20260515131301). Idempotent: only insert when no ON_HOLD row exists, so re-running or
            // applying to a DB that already has it is safe. Shared master.Statuses row → available to
            // every request type.
            migrationBuilder.Sql($@"
                IF NOT EXISTS (SELECT 1 FROM master.Statuses WHERE Code = 'ON_HOLD')
                BEGIN
                    INSERT INTO master.Statuses
                        (Id, nameEn, nameAr, Code, orderNumber, showInDropdown, iconName, CreatedDate, CreatedBy, IsDeleted)
                    VALUES
                        ('{OnHoldStatusId}', 'On Hold', N'قيد الانتظار', 'ON_HOLD', 6, 1, 'pause',
                         SYSDATETIMEOFFSET(), '00000000-0000-0000-0000-000000000000', 0);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only remove the seeded row when nothing references it, to avoid FK violations.
            migrationBuilder.Sql($@"
                IF NOT EXISTS (SELECT 1 FROM business.EngineerRequests WHERE StatusId = '{OnHoldStatusId}')
                   AND NOT EXISTS (SELECT 1 FROM business.EngineerRequestActivites WHERE StatusId = '{OnHoldStatusId}')
                BEGIN
                    DELETE FROM master.Statuses WHERE Id = '{OnHoldStatusId}';
                END
            ");
        }
    }
}
