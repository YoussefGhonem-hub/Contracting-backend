using Contracting.Domain.Entities.master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Master
{
    public class SpecialFieldConfiguration : IEntityTypeConfiguration<SpecialField>
    {
        public void Configure(EntityTypeBuilder<SpecialField> builder)
        {
            builder.ToTable("SpecialFields", "master");

            builder.Property(sf => sf.name)
                .HasMaxLength(200);

            builder.Property(sf => sf.fieldType)
                .HasMaxLength(100);
        }
    }
}
