using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class ChatGroupConfiguration : IEntityTypeConfiguration<ChatGroup>
{
    public void Configure(EntityTypeBuilder<ChatGroup> builder)
    {
        builder.ToTable("ChatGroups", "client");
        builder.Property(g => g.Name).HasMaxLength(300);

        builder.HasIndex(g => g.ProjectId).IsUnique();

        builder.HasOne(g => g.Project)
            .WithMany()
            .HasForeignKey(g => g.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
