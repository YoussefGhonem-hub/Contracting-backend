using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class reupdateuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Engineers_AspNetUsers_ApplicationUserId",
                table: "Engineers");

            migrationBuilder.AddForeignKey(
                name: "FK_Engineers_AspNetUsers_ApplicationUserId",
                table: "Engineers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Engineers_AspNetUsers_ApplicationUserId",
                table: "Engineers");

            migrationBuilder.AddForeignKey(
                name: "FK_Engineers_AspNetUsers_ApplicationUserId",
                table: "Engineers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
