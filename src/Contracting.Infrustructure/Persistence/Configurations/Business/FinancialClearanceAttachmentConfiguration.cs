using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class FinancialClearanceAttachmentConfiguration : IEntityTypeConfiguration<FinancialClearanceAttachment>
    {
        public void Configure(EntityTypeBuilder<FinancialClearanceAttachment> builder)
        {
            builder.ToTable("FinancialClearanceAttachments", "business");
            builder.Property(a => a.Key).HasMaxLength(500);
            builder.Property(a => a.FileName).HasMaxLength(500);
            builder.Property(a => a.Extension).HasMaxLength(50);
            builder.Property(a => a.Url).HasMaxLength(2000);
            builder.Property(a => a.AttachmentType).HasMaxLength(100);
        }
    }
}
