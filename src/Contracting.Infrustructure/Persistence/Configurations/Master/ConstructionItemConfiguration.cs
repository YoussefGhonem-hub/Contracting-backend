using Contracting.Domain.Entities.master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Master
{
    public class ConstructionItemConfiguration : IEntityTypeConfiguration<ConstructionItem>
    {
        public void Configure(EntityTypeBuilder<ConstructionItem> builder)
        {
            builder.ToTable("ConstructionItems", "master");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.nameEn).HasMaxLength(200);
            builder.Property(c => c.nameAr).HasMaxLength(200);
            builder.Property(c => c.Unit).HasMaxLength(100);
            builder.Property(c => c.ItemCode).HasMaxLength(100);
        }
    }
}
