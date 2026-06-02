using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingInfoNeedsUpdateStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Replace "On Hold" with "Missing Information" — updates the existing row in-place
            // so all requests that were On Hold automatically become Missing Information (same Id)
            migrationBuilder.Sql(@"
                UPDATE master.Statuses
                SET nameEn         = 'Missing Information',
                    nameAr         = N'معلومات ناقصة',
                    Code           = 'missing_information',
                    showInDropdown = 1
                WHERE LOWER(Code) = 'on_hold'
                   OR LOWER(Code) = 'onhold'
                   OR LOWER(nameEn) LIKE '%on hold%';
            ");

            // Clean up any duplicate Missing Information row that may have been seeded separately
            migrationBuilder.Sql(@"
                DELETE FROM master.Statuses
                WHERE LOWER(Code) = 'missing_information'
                  AND CreatedBy = CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore "On Hold" status from the record that was renamed
            migrationBuilder.Sql(@"
                UPDATE master.Statuses
                SET nameEn         = 'On Hold',
                    nameAr         = N'قيد الانتظار',
                    Code           = 'ON_HOLD',
                    showInDropdown = 1
                WHERE LOWER(Code) = 'missing_information';
            ");
        }
    }
}
