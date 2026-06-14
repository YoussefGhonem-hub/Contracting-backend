using Contracting.Domain.Entities.master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Master
{
    public class ConstructionItemUnitConfiguration : IEntityTypeConfiguration<ConstructionItemUnit>
    {
        public void Configure(EntityTypeBuilder<ConstructionItemUnit> builder)
        {
            builder.ToTable("ConstructionItemUnits", "master");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.nameEn).HasMaxLength(200);
            builder.Property(u => u.nameAr).HasMaxLength(200);

            builder.HasOne(u => u.ConstructionItem)
                .WithMany(c => c.Units)
                .HasForeignKey(u => u.ConstructionItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
