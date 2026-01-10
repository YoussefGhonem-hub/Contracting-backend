using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerRequestAttachmentConfiguration : IEntityTypeConfiguration<EngineerRequestAttachment>
    {
        public void Configure(EntityTypeBuilder<EngineerRequestAttachment> builder)
        {
            builder.ToTable("EngineerRequestAttachments", "business");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Key).HasMaxLength(500);
            builder.Property(a => a.FileName).HasMaxLength(255);
            builder.Property(a => a.Extension).HasMaxLength(50);
            builder.Property(a => a.Url).HasMaxLength(1000);

            builder.HasOne(a => a.EngineerRequest)
                   .WithMany(r => r.EngineerRequestAttachments)
                   .HasForeignKey(a => a.EngineerRequestId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.EngineerRequestNotes)
                   .WithMany(n => n.EngineerRequestAttachments)
                   .HasForeignKey(a => a.EngineerRequestNotesId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.EngineerRequestId);
            builder.HasIndex(a => a.EngineerRequestNotesId);
        }
    }
}
