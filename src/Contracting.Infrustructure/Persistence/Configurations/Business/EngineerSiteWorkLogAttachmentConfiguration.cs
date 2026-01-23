using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerSiteWorkLogAttachmentConfiguration : IEntityTypeConfiguration<EngineerSiteWorkLogAttachment>
    {
        public void Configure(EntityTypeBuilder<EngineerSiteWorkLogAttachment> builder)
        {
            builder.ToTable("EngineerSiteWorkLogAttachments", "business");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Key).HasMaxLength(500);
            builder.Property(a => a.FileName).HasMaxLength(255);
            builder.Property(a => a.Extension).HasMaxLength(50);
            builder.Property(a => a.Url).HasMaxLength(1000);

            builder.HasOne(a => a.EngineerSiteWorkLog)
                .WithMany(w => w.Attachments)
                .HasForeignKey(a => a.EngineerSiteWorkLogId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.EngineerSiteWorkLogId);
        }
    }
}
