using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerSiteReportAttachmentConfiguration : IEntityTypeConfiguration<EngineerSiteReportAttachment>
    {
        public void Configure(EntityTypeBuilder<EngineerSiteReportAttachment> builder)
        {
            builder.ToTable("EngineerSiteReportAttachments", "business");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Key).HasMaxLength(500);
            builder.Property(a => a.FileName).HasMaxLength(500);
            builder.Property(a => a.Extension).HasMaxLength(20);
            builder.Property(a => a.Url).HasMaxLength(2000);

            builder.HasOne(a => a.EngineerSiteReport)
                .WithMany(r => r.Attachments)
                .HasForeignKey(a => a.EngineerSiteReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.EngineerSiteReportId);
        }
    }
}
