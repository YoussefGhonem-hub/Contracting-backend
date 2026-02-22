using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerRequestSpecialFieldValueConfiguration : IEntityTypeConfiguration<EngineerRequestSpecialFieldValue>
    {
        public void Configure(EntityTypeBuilder<EngineerRequestSpecialFieldValue> builder)
        {
            builder.ToTable("EngineerRequestSpecialFieldValues", "business");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.value)
                .HasMaxLength(1000);

            builder.HasOne(x => x.EngineerRequest)
                .WithMany(r => r.SpecialFieldValues)
                .HasForeignKey(x => x.EngineerRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.DepartmentSpecialField)
                .WithMany()
                .HasForeignKey(x => x.DepartmentSpecialFieldId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
