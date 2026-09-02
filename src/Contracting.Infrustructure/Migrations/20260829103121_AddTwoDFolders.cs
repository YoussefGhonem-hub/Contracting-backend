using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoDFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TwoDFolders",
                schema: "client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CoverKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CoverUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_TwoDFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwoDFolders_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "master",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TwoDFolders_Users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalSchema: "security",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TwoDImages",
                schema: "client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_TwoDImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwoDImages_TwoDFolders_FolderId",
                        column: x => x.FolderId,
                        principalSchema: "client",
                        principalTable: "TwoDFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TwoDImages_Users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalSchema: "security",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TwoDFolders_IsDeleted",
                schema: "client",
                table: "TwoDFolders",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TwoDFolders_ProjectId",
                schema: "client",
                table: "TwoDFolders",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TwoDFolders_UploadedBy",
                schema: "client",
                table: "TwoDFolders",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TwoDImages_FolderId",
                schema: "client",
                table: "TwoDImages",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_TwoDImages_IsDeleted",
                schema: "client",
                table: "TwoDImages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TwoDImages_UploadedBy",
                schema: "client",
                table: "TwoDImages",
                column: "UploadedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TwoDImages",
                schema: "client");

            migrationBuilder.DropTable(
                name: "TwoDFolders",
                schema: "client");
        }
    }
}
