using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class ReportConstructionItemWorkerConfiguration : IEntityTypeConfiguration<ReportConstructionItemWorker>
    {
        public void Configure(EntityTypeBuilder<ReportConstructionItemWorker> builder)
        {
            builder.ToTable("ReportConstructionItemWorkers", "business");

            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.EngineerSiteReport)
                .WithMany(r => r.Workers)
                .HasForeignKey(w => w.EngineerSiteReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(w => w.ConstructionItem)
                .WithMany()
                .HasForeignKey(w => w.ConstructionItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(w => w.EngineerSiteReportId);
            builder.HasIndex(w => w.ConstructionItemId);
        }
    }
}
