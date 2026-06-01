using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class ProjectDrawingConfiguration : IEntityTypeConfiguration<ProjectDrawing>
{
    public void Configure(EntityTypeBuilder<ProjectDrawing> builder)
    {
        builder.ToTable("ProjectDrawings", "client");
        builder.Property(d => d.Title).HasMaxLength(500);
        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.FileName).HasMaxLength(500);
        builder.Property(d => d.Extension).HasMaxLength(20);
        builder.Property(d => d.Url).HasMaxLength(2000);

        builder.HasOne(d => d.Project)
            .WithMany()
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.UploadedByUser)
            .WithMany()
            .HasForeignKey(d => d.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
