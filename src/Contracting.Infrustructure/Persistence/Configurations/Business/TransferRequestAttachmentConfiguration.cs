using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class TransferRequestAttachmentConfiguration : IEntityTypeConfiguration<TransferRequestAttachment>
    {
        public void Configure(EntityTypeBuilder<TransferRequestAttachment> builder)
        {
            builder.ToTable("TransferRequestAttachments", "business");
            builder.Property(a => a.Key).HasMaxLength(500);
            builder.Property(a => a.FileName).HasMaxLength(500);
            builder.Property(a => a.Extension).HasMaxLength(50);
            builder.Property(a => a.Url).HasMaxLength(2000);
        }
    }
}
