using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class VariationOrderAttachmentConfiguration : IEntityTypeConfiguration<VariationOrderAttachment>
{
    public void Configure(EntityTypeBuilder<VariationOrderAttachment> builder)
    {
        builder.ToTable("VariationOrderAttachments", "client");
        builder.Property(a => a.FileName).HasMaxLength(500);
        builder.Property(a => a.Extension).HasMaxLength(20);
        builder.Property(a => a.Url).HasMaxLength(2000);

        builder.HasOne(a => a.VariationOrder)
            .WithMany(v => v.Attachments)
            .HasForeignKey(a => a.VariationOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
