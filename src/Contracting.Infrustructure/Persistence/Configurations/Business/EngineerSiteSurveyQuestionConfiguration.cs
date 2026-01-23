using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerSiteSurveyQuestionConfiguration : IEntityTypeConfiguration<EngineerSiteSurveyQuestion>
    {
        public void Configure(EntityTypeBuilder<EngineerSiteSurveyQuestion> builder)
        {
            builder.ToTable("EngineerSiteSurveyQuestions", "business");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.question).HasMaxLength(1000);
            builder.Property(s => s.answer).HasMaxLength(20);
            builder.Property(s => s.description).HasMaxLength(1000);

            builder.HasOne(s => s.EngineerSiteReport)
                .WithMany(r => r.SurveyQuestions)
                .HasForeignKey(s => s.EngineerSiteReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Template)
                .WithMany()
                .HasForeignKey(s => s.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(s => s.EngineerSiteReportId);
            builder.HasIndex(s => s.TemplateId);
        }
    }
}
