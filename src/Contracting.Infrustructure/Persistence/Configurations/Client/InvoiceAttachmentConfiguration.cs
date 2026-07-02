using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class InvoiceAttachmentConfiguration : IEntityTypeConfiguration<InvoiceAttachment>
{
    public void Configure(EntityTypeBuilder<InvoiceAttachment> builder)
    {
        builder.ToTable("InvoiceAttachments", "client");
        builder.Property(a => a.Key).HasMaxLength(500);
        builder.Property(a => a.FileName).HasMaxLength(500);
        builder.Property(a => a.Extension).HasMaxLength(50);
        builder.Property(a => a.Url).HasMaxLength(2000);

        builder.HasOne(a => a.ProjectInvoice)
            .WithMany(i => i.Attachments)
            .HasForeignKey(a => a.ProjectInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
