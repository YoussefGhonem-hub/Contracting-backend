using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerRequestNotesConfiguration : IEntityTypeConfiguration<EngineerRequestNotes>
    {
        public void Configure(EntityTypeBuilder<EngineerRequestNotes> builder)
        {
            builder.ToTable("EngineerRequestNotes", "business");
            builder.Property(n => n.note).HasMaxLength(2000);
        }
    }
}
