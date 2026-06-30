using Contracting.Domain.Entities.helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Helper
{
    public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
    {
        public void Configure(EntityTypeBuilder<NotificationLog> builder)
        {
            builder.ToTable("NotificationLogs", "helper");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Token)
                .HasMaxLength(500);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Body)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.IsSent)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(x => x.Type)
                .HasMaxLength(50);

            builder.Property(x => x.SentAt)
                .IsRequired();

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .IsRequired();

            // Engineer FK
            builder.HasOne(x => x.Engineer)
                .WithMany()
                .HasForeignKey(x => x.EngineerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for better query performance
            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("idx_NotificationLogs_UserId");

            builder.HasIndex(x => x.IsSent)
                .HasDatabaseName("idx_NotificationLogs_IsSent");

            builder.HasIndex(x => x.IsRead)
                .HasDatabaseName("idx_NotificationLogs_IsRead");

            builder.HasIndex(x => x.EngineerId)
                .HasDatabaseName("idx_NotificationLogs_EngineerId");

            builder.HasIndex(x => x.ChatGroupId)
                .HasDatabaseName("idx_NotificationLogs_ChatGroupId");

            builder.HasIndex(x => x.CreatedDate)
                .HasDatabaseName("idx_NotificationLogs_CreatedDate");
        }
    }
}
