using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerSiteWorkLogConfiguration : IEntityTypeConfiguration<EngineerSiteWorkLog>
    {
        public void Configure(EntityTypeBuilder<EngineerSiteWorkLog> builder)
        {
            builder.ToTable("EngineerSiteWorkLogs", "business");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.name).HasMaxLength(200);
            builder.Property(w => w.description).HasMaxLength(2000);
            builder.Property(w => w.quantity).HasColumnType("decimal(18,2)");
            builder.Property(w => w.totalHours).HasColumnType("decimal(18,2)");
            builder.Property(w => w.totalHoursToDate).HasColumnType("decimal(18,2)");

            builder.HasOne(w => w.EngineerSiteReport)
                .WithMany()
                .HasForeignKey(w => w.EngineerSiteReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(w => w.EngineerSiteReportId);
        }
    }
}
