using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class TwoDImageConfiguration : IEntityTypeConfiguration<TwoDImage>
{
    public void Configure(EntityTypeBuilder<TwoDImage> builder)
    {
        builder.ToTable("TwoDImages", "client");

        builder.Property(i => i.Key).HasMaxLength(1000);
        builder.Property(i => i.FileName).HasMaxLength(500);
        builder.Property(i => i.Extension).HasMaxLength(20);
        builder.Property(i => i.Url).HasMaxLength(2000);

        builder.HasOne(i => i.UploadedByUser)
            .WithMany()
            .HasForeignKey(i => i.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
