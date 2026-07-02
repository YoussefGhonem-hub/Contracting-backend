using Contracting.Domain.Entities.master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Master
{
    public class EngineerProjectFeatureConfiguration : IEntityTypeConfiguration<EngineerProjectFeature>
    {
        public void Configure(EntityTypeBuilder<EngineerProjectFeature> builder)
        {
            builder.ToTable("EngineerProjectFeatures", "master");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Feature)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(f => new { f.EngineerProjectId, f.Feature })
                .IsUnique();

            builder.HasOne(f => f.EngineerProject)
                .WithMany(ep => ep.Features)
                .HasForeignKey(f => f.EngineerProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
