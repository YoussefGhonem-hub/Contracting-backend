using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class ClientMonthlyReportAttachmentConfiguration : IEntityTypeConfiguration<ClientMonthlyReportAttachment>
{
    public void Configure(EntityTypeBuilder<ClientMonthlyReportAttachment> builder)
    {
        builder.ToTable("ClientMonthlyReportAttachments", "client");
        builder.Property(a => a.FileName).HasMaxLength(500);
        builder.Property(a => a.Extension).HasMaxLength(20);
        builder.Property(a => a.Url).HasMaxLength(2000);

        builder.HasOne(a => a.ClientMonthlyReport)
            .WithMany(r => r.Attachments)
            .HasForeignKey(a => a.ClientMonthlyReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
