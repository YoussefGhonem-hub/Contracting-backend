using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracting.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class ReprotsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId",
                schema: "business",
                table: "EngineerSiteSurveyQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EngineerSiteSurveyQuestions_TemplateId",
                schema: "business",
                table: "EngineerSiteSurveyQuestions",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_EngineerSiteSurveyQuestions_EngineerSiteSurveyQuestionTemplates_TemplateId",
                schema: "business",
                table: "EngineerSiteSurveyQuestions",
                column: "TemplateId",
                principalSchema: "business",
                principalTable: "EngineerSiteSurveyQuestionTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EngineerSiteSurveyQuestions_EngineerSiteSurveyQuestionTemplates_TemplateId",
                schema: "business",
                table: "EngineerSiteSurveyQuestions");

            migrationBuilder.DropIndex(
                name: "IX_EngineerSiteSurveyQuestions_TemplateId",
                schema: "business",
                table: "EngineerSiteSurveyQuestions");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                schema: "business",
                table: "EngineerSiteSurveyQuestions");
        }
    }
}
