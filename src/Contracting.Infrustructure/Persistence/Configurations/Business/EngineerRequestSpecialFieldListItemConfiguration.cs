using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerRequestSpecialFieldListItemConfiguration : IEntityTypeConfiguration<EngineerRequestSpecialFieldListItem>
    {
        public void Configure(EntityTypeBuilder<EngineerRequestSpecialFieldListItem> builder)
        {
            builder.ToTable("EngineerRequestSpecialFieldListItems", "business");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.value)
                .HasMaxLength(1000);

            builder.HasOne(x => x.EngineerRequest)
                .WithMany(r => r.SpecialFieldListItems)
                .HasForeignKey(x => x.EngineerRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.DepartmentSpecialField)
                .WithMany()
                .HasForeignKey(x => x.DepartmentSpecialFieldId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
