using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class ChatMessageAttachmentConfiguration : IEntityTypeConfiguration<ChatMessageAttachment>
{
    public void Configure(EntityTypeBuilder<ChatMessageAttachment> builder)
    {
        builder.ToTable("ChatMessageAttachments", "client");
        builder.Property(a => a.FileName).HasMaxLength(500);
        builder.Property(a => a.Extension).HasMaxLength(20);
        builder.Property(a => a.Url).HasMaxLength(2000);

        builder.HasOne(a => a.ChatMessage)
            .WithMany(m => m.Attachments)
            .HasForeignKey(a => a.ChatMessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
