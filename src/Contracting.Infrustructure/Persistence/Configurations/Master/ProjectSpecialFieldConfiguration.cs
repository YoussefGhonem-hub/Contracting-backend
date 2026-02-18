using Contracting.Domain.Entities.master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Master
{
    public class ProjectSpecialFieldConfiguration : IEntityTypeConfiguration<ProjectSpecialField>
    {
        public void Configure(EntityTypeBuilder<ProjectSpecialField> builder)
        {
            builder.ToTable("ProjectSpecialFields", "master");

            builder.HasKey(psf => psf.Id);

            builder.HasOne(psf => psf.Project)
                .WithMany(p => p.ProjectSpecialFields)
                .HasForeignKey(psf => psf.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(psf => psf.SpecialField)
                .WithMany(sf => sf.ProjectSpecialFields)
                .HasForeignKey(psf => psf.SpecialFieldId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
