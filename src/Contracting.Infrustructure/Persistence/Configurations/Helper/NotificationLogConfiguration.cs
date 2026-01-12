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

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(x => x.SentAt)
                .IsRequired();

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .IsRequired();

            // Indexes for better query performance
            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("idx_NotificationLogs_UserId");

            builder.HasIndex(x => x.IsSent)
                .HasDatabaseName("idx_NotificationLogs_IsSent");

            builder.HasIndex(x => x.CreatedDate)
                .HasDatabaseName("idx_NotificationLogs_CreatedDate");
        }
    }
}
