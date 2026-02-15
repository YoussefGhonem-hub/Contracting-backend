using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerSiteEquipmentConfiguration : IEntityTypeConfiguration<EngineerSiteEquipment>
    {
        public void Configure(EntityTypeBuilder<EngineerSiteEquipment> builder)
        {
            builder.ToTable("EngineerSiteEquipments", "business");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.name).HasMaxLength(200);
            builder.Property(e => e.condition).HasMaxLength(500);
            builder.Property(e => e.notes).HasMaxLength(1000);
            builder.Property(e => e.quantity).HasColumnType("decimal(18,2)");
            builder.Property(e => e.hoursUsed).HasColumnType("decimal(18,2)");

            builder.HasOne(e => e.EngineerSiteReport)
                .WithMany()
                .HasForeignKey(e => e.EngineerSiteReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.EngineerSiteReportId);
        }
    }
}
