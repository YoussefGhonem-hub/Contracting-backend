using Contracting.Domain.Entities.master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Master
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects", "master");
            builder.Property(p => p.nameEn).HasMaxLength(200);
            builder.Property(p => p.nameAr).HasMaxLength(200);
        }
    }
}
