using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerSiteReportConfiguration : IEntityTypeConfiguration<EngineerSiteReport>
    {
        public void Configure(EntityTypeBuilder<EngineerSiteReport> builder)
        {
            builder.ToTable("EngineerSiteReports", "business");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.ReportDate).IsRequired();
            builder.Property(r => r.GeneralNotes).HasMaxLength(4000);
            builder.Property(r => r.SiteSafetyObservations).HasMaxLength(4000);
            builder.Property(r => r.QualityControlObservations).HasMaxLength(4000);

            builder.HasOne(r => r.Engineer)
                .WithMany()
                .HasForeignKey(r => r.EngineerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Project)
                .WithMany()
                .HasForeignKey(r => r.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(r => r.EngineerId);
            builder.HasIndex(r => r.ProjectId);
            builder.HasIndex(r => r.ReportDate);
        }
    }
}
