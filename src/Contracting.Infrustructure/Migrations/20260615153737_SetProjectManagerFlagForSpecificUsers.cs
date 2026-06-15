using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class SetProjectManagerFlagForSpecificUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ep
                SET ep.IsProjectManager = 1
                FROM master.EngineerProjects ep
                INNER JOIN master.Engineers e ON e.Id = ep.EngineerId
                INNER JOIN security.Users u ON u.Id = e.ApplicationUserId
                WHERE u.Email IN (
                    'amr.sherif1@example.com',
                    'Islam.youssef@company.com',
                    'mohamed.adel@company.com'
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ep
                SET ep.IsProjectManager = 0
                FROM master.EngineerProjects ep
                INNER JOIN master.Engineers e ON e.Id = ep.EngineerId
                INNER JOIN security.Users u ON u.Id = e.ApplicationUserId
                WHERE u.Email IN (
                    'amr.sherif1@example.com',
                    'Islam.youssef@company.com',
                    'mohamed.adel@company.com'
                )
            ");
        }
    }
}
