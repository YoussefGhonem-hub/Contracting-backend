using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class ClientProjectConfiguration : IEntityTypeConfiguration<ClientProject>
{
    public void Configure(EntityTypeBuilder<ClientProject> builder)
    {
        builder.ToTable("ClientProjects", "client");

        builder.HasOne(cp => cp.Client)
            .WithMany(c => c.ClientProjects)
            .HasForeignKey(cp => cp.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.Project)
            .WithMany()
            .HasForeignKey(cp => cp.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
