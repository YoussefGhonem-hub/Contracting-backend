using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class FixClientStatusEnumValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // VariationOrders: migrate old CLIENT_STATUS_* enum names to current enum names
            migrationBuilder.Sql(@"
                UPDATE [client].[VariationOrders]
                SET [Status] = CASE [Status]
                    WHEN 'CLIENT_STATUS_PENDING'  THEN 'Pending'
                    WHEN 'CLIENT_STATUS_APPROVED' THEN 'Approved'
                    WHEN 'CLIENT_STATUS_REJECTED' THEN 'Rejected'
                    ELSE [Status]
                END
                WHERE [Status] IN ('CLIENT_STATUS_PENDING', 'CLIENT_STATUS_APPROVED', 'CLIENT_STATUS_REJECTED');
            ");

            // ProjectInvoices: migrate old CLIENT_STATUS_* enum names to current enum names
            migrationBuilder.Sql(@"
                UPDATE [client].[ProjectInvoices]
                SET [Status] = CASE [Status]
                    WHEN 'CLIENT_STATUS_PENDING'       THEN 'Pending'
                    WHEN 'CLIENT_STATUS_PAID'          THEN 'Paid'
                    WHEN 'CLIENT_STATUS_PARTIALLYPAID' THEN 'PartiallyPaid'
                    ELSE [Status]
                END
                WHERE [Status] IN ('CLIENT_STATUS_PENDING', 'CLIENT_STATUS_PAID', 'CLIENT_STATUS_PARTIALLYPAID');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [client].[VariationOrders]
                SET [Status] = CASE [Status]
                    WHEN 'Pending'  THEN 'CLIENT_STATUS_PENDING'
                    WHEN 'Approved' THEN 'CLIENT_STATUS_APPROVED'
                    WHEN 'Rejected' THEN 'CLIENT_STATUS_REJECTED'
                    ELSE [Status]
                END
                WHERE [Status] IN ('Pending', 'Approved', 'Rejected');
            ");

            migrationBuilder.Sql(@"
                UPDATE [client].[ProjectInvoices]
                SET [Status] = CASE [Status]
                    WHEN 'Pending'       THEN 'CLIENT_STATUS_PENDING'
                    WHEN 'Paid'          THEN 'CLIENT_STATUS_PAID'
                    WHEN 'PartiallyPaid' THEN 'CLIENT_STATUS_PARTIALLYPAID'
                    ELSE [Status]
                END
                WHERE [Status] IN ('Pending', 'Paid', 'PartiallyPaid');
            ");
        }
    }
}
