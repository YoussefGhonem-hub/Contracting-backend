using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddThreeDFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThreeDFolders",
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
                    table.PrimaryKey("PK_ThreeDFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThreeDFolders_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "master",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThreeDFolders_Users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalSchema: "security",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThreeDImages",
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
                    table.PrimaryKey("PK_ThreeDImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThreeDImages_ThreeDFolders_FolderId",
                        column: x => x.FolderId,
                        principalSchema: "client",
                        principalTable: "ThreeDFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThreeDImages_Users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalSchema: "security",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThreeDFolders_IsDeleted",
                schema: "client",
                table: "ThreeDFolders",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ThreeDFolders_ProjectId",
                schema: "client",
                table: "ThreeDFolders",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreeDFolders_UploadedBy",
                schema: "client",
                table: "ThreeDFolders",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ThreeDImages_FolderId",
                schema: "client",
                table: "ThreeDImages",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreeDImages_IsDeleted",
                schema: "client",
                table: "ThreeDImages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ThreeDImages_UploadedBy",
                schema: "client",
                table: "ThreeDImages",
                column: "UploadedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThreeDImages",
                schema: "client");

            migrationBuilder.DropTable(
                name: "ThreeDFolders",
                schema: "client");
        }
    }
}
