using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerSiteSurveyQuestionTemplateConfiguration : IEntityTypeConfiguration<EngineerSiteSurveyQuestionTemplate>
    {
        public void Configure(EntityTypeBuilder<EngineerSiteSurveyQuestionTemplate> builder)
        {
            builder.ToTable("EngineerSiteSurveyQuestionTemplates", "business");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.question).HasMaxLength(1000);
            builder.Property(s => s.order).IsRequired();
            builder.Property(s => s.isActive).IsRequired();

            builder.HasIndex(s => s.isActive);
            builder.HasIndex(s => s.order);
        }
    }
}
