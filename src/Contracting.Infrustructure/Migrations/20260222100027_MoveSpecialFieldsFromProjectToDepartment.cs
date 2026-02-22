using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveSpecialFieldsFromProjectToDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EngineerRequestSpecialFieldValues_ProjectSpecialFields_ProjectSpecialFieldId",
                schema: "business",
                table: "EngineerRequestSpecialFieldValues");

            migrationBuilder.DropColumn(
                name: "hasSpecialFields",
                schema: "master",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "ProjectSpecialFieldId",
                schema: "business",
                table: "EngineerRequestSpecialFieldValues",
                newName: "DepartmentSpecialFieldId");

            migrationBuilder.RenameIndex(
                name: "IX_EngineerRequestSpecialFieldValues_ProjectSpecialFieldId",
                schema: "business",
                table: "EngineerRequestSpecialFieldValues",
                newName: "IX_EngineerRequestSpecialFieldValues_DepartmentSpecialFieldId");

            migrationBuilder.AddColumn<bool>(
                name: "hasSpecialFields",
                schema: "master",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DepartmentSpecialFields",
                schema: "master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecialFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_DepartmentSpecialFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentSpecialFields_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "master",
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentSpecialFields_SpecialFields_SpecialFieldId",
                        column: x => x.SpecialFieldId,
                        principalSchema: "master",
                        principalTable: "SpecialFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentSpecialFields_DepartmentId",
                schema: "master",
                table: "DepartmentSpecialFields",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentSpecialFields_IsDeleted",
                schema: "master",
                table: "DepartmentSpecialFields",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentSpecialFields_SpecialFieldId",
                schema: "master",
                table: "DepartmentSpecialFields",
                column: "SpecialFieldId");

            // Migrate existing data: copy ProjectSpecialFields into DepartmentSpecialFields
            // Map ProjectId to DepartmentId through EngineerRequest (which has both ProjectId and DepartmentId)
            // For ProjectSpecialFields not referenced by any EngineerRequest, use the first Department in the same Branch as the Project
            migrationBuilder.Sql(@"
                INSERT INTO [master].[DepartmentSpecialFields] 
                    ([Id], [DepartmentId], [SpecialFieldId], [value], [CreatedDate], [ModifiedDate], [CreatedBy], [ModifiedBy], [IsDeleted], [DeletedDate], [DeletedBy])
                SELECT 
                    psf.[Id],
                    COALESCE(
                        (SELECT TOP 1 er.[DepartmentId] 
                         FROM [business].[EngineerRequests] er 
                         INNER JOIN [business].[EngineerRequestSpecialFieldValues] ersv ON ersv.[EngineerRequestId] = er.[Id]
                         WHERE ersv.[DepartmentSpecialFieldId] = psf.[Id]
                         AND er.[DepartmentId] IS NOT NULL),
                        (SELECT TOP 1 d.[Id] 
                         FROM [master].[Departments] d 
                         WHERE d.[BranchId] = p.[BranchId] AND d.[IsDeleted] = 0),
                        (SELECT TOP 1 d.[Id] FROM [master].[Departments] d WHERE d.[IsDeleted] = 0)
                    ) AS [DepartmentId],
                    psf.[SpecialFieldId],
                    psf.[value],
                    psf.[CreatedDate],
                    psf.[ModifiedDate],
                    psf.[CreatedBy],
                    psf.[ModifiedBy],
                    psf.[IsDeleted],
                    psf.[DeletedDate],
                    psf.[DeletedBy]
                FROM [master].[ProjectSpecialFields] psf
                INNER JOIN [master].[Projects] p ON p.[Id] = psf.[ProjectId]
                WHERE psf.[IsDeleted] = 0;

                -- Set hasSpecialFields on Departments that now have special fields
                UPDATE d SET d.[hasSpecialFields] = 1
                FROM [master].[Departments] d
                WHERE EXISTS (SELECT 1 FROM [master].[DepartmentSpecialFields] dsf WHERE dsf.[DepartmentId] = d.[Id] AND dsf.[IsDeleted] = 0);
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_EngineerRequestSpecialFieldValues_DepartmentSpecialFields_DepartmentSpecialFieldId",
                schema: "business",
                table: "EngineerRequestSpecialFieldValues",
                column: "DepartmentSpecialFieldId",
                principalSchema: "master",
                principalTable: "DepartmentSpecialFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EngineerRequestSpecialFieldValues_DepartmentSpecialFields_DepartmentSpecialFieldId",
                schema: "business",
                table: "EngineerRequestSpecialFieldValues");

            migrationBuilder.DropTable(
                name: "DepartmentSpecialFields",
                schema: "master");

            migrationBuilder.DropColumn(
                name: "hasSpecialFields",
                schema: "master",
                table: "Departments");

            migrationBuilder.RenameColumn(
                name: "DepartmentSpecialFieldId",
                schema: "business",
                table: "EngineerRequestSpecialFieldValues",
                newName: "ProjectSpecialFieldId");

            migrationBuilder.RenameIndex(
                name: "IX_EngineerRequestSpecialFieldValues_DepartmentSpecialFieldId",
                schema: "business",
                table: "EngineerRequestSpecialFieldValues",
                newName: "IX_EngineerRequestSpecialFieldValues_ProjectSpecialFieldId");

            migrationBuilder.AddColumn<bool>(
                name: "hasSpecialFields",
                schema: "master",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_EngineerRequestSpecialFieldValues_ProjectSpecialFields_ProjectSpecialFieldId",
                schema: "business",
                table: "EngineerRequestSpecialFieldValues",
                column: "ProjectSpecialFieldId",
                principalSchema: "master",
                principalTable: "ProjectSpecialFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
