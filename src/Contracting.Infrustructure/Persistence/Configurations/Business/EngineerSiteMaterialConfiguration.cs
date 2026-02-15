using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerSiteMaterialConfiguration : IEntityTypeConfiguration<EngineerSiteMaterial>
    {
        public void Configure(EntityTypeBuilder<EngineerSiteMaterial> builder)
        {
            builder.ToTable("EngineerSiteMaterials", "business");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.name).HasMaxLength(200);
            builder.Property(m => m.usage).HasMaxLength(1000);
            builder.Property(m => m.unit).HasMaxLength(50);
            builder.Property(m => m.notes).HasMaxLength(1000);
            builder.Property(m => m.quantity).HasColumnType("decimal(18,2)");
            builder.Property(m => m.unitCost).HasColumnType("decimal(18,2)");
            builder.Property(m => m.totalCost).HasColumnType("decimal(18,2)");

            builder.HasOne(m => m.EngineerSiteReport)
                .WithMany()
                .HasForeignKey(m => m.EngineerSiteReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => m.EngineerSiteReportId);
        }
    }
}
