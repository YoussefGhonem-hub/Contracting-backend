using Contracting.Domain.Entities.master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Master
{
    public class EngineerProjectConfiguration : IEntityTypeConfiguration<EngineerProject>
    {
        public void Configure(EntityTypeBuilder<EngineerProject> builder)
        {
            builder.ToTable("EngineerProjects", "master");

            builder.HasKey(ep => ep.Id);

            builder.HasOne(ep => ep.Engineer)
                .WithMany(e => e.EngineerProjects)
                .HasForeignKey(ep => ep.EngineerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ep => ep.Project)
                .WithMany(p => p.EngineerProjects)
                .HasForeignKey(ep => ep.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
