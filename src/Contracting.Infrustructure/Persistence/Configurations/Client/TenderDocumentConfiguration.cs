using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class TenderDocumentConfiguration : IEntityTypeConfiguration<TenderDocument>
{
    public void Configure(EntityTypeBuilder<TenderDocument> builder)
    {
        builder.ToTable("TenderDocuments", "client");
        builder.Property(t => t.Title).HasMaxLength(500);
        builder.Property(t => t.FileName).HasMaxLength(500);
        builder.Property(t => t.Extension).HasMaxLength(20);
        builder.Property(t => t.Url).HasMaxLength(2000);

        builder.HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.UploadedByUser)
            .WithMany()
            .HasForeignKey(t => t.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
