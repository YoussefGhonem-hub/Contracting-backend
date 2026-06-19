using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class ThreeDFolderConfiguration : IEntityTypeConfiguration<ThreeDFolder>
{
    public void Configure(EntityTypeBuilder<ThreeDFolder> builder)
    {
        builder.ToTable("ThreeDFolders", "client");

        builder.Property(f => f.Title).HasMaxLength(500).IsRequired();
        builder.Property(f => f.CoverKey).HasMaxLength(1000);
        builder.Property(f => f.CoverUrl).HasMaxLength(2000);

        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.UploadedByUser)
            .WithMany()
            .HasForeignKey(f => f.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Images)
            .WithOne(i => i.Folder)
            .HasForeignKey(i => i.FolderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
