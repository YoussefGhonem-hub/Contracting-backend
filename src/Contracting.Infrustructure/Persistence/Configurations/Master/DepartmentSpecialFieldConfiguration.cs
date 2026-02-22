using Contracting.Domain.Entities.master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Master
{
    public class DepartmentSpecialFieldConfiguration : IEntityTypeConfiguration<DepartmentSpecialField>
    {
        public void Configure(EntityTypeBuilder<DepartmentSpecialField> builder)
        {
            builder.ToTable("DepartmentSpecialFields", "master");

            builder.HasKey(dsf => dsf.Id);

            builder.HasOne(dsf => dsf.Department)
                .WithMany(d => d.DepartmentSpecialFields)
                .HasForeignKey(dsf => dsf.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(dsf => dsf.SpecialField)
                .WithMany(sf => sf.DepartmentSpecialFields)
                .HasForeignKey(dsf => dsf.SpecialFieldId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
