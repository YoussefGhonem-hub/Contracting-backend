using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerRequestActiviteConfiguration : IEntityTypeConfiguration<EngineerRequestActivite>
    {
        public void Configure(EntityTypeBuilder<EngineerRequestActivite> builder)
        {
            builder.ToTable("EngineerRequestActivites", "business");
            builder.Property(a => a.description).HasMaxLength(2000);
        }
    }
}
