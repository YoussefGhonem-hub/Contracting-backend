using Contracting.Domain.Entities.helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Helper
{
    public class ExceptionLogConfiguration : IEntityTypeConfiguration<ExceptionLog>
    {
        public void Configure(EntityTypeBuilder<ExceptionLog> builder)
        {
            builder.ToTable("ExceptionLogs", "helper");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.StackTrace)
                .HasMaxLength(4000);

            builder.Property(x => x.InnerExceptionMessage)
                .HasMaxLength(2000);

            builder.Property(x => x.InnerExceptionStackTrace)
                .HasMaxLength(4000);

            builder.Property(x => x.ExceptionType)
                .HasMaxLength(500);

            builder.Property(x => x.HttpMethod)
                .HasMaxLength(10);

            builder.Property(x => x.RequestPath)
                .HasMaxLength(2000);

            builder.Property(x => x.QueryString)
                .HasMaxLength(2000);

            builder.Property(x => x.UserId)
                .HasMaxLength(50);

            builder.Property(x => x.UserAgent)
                .HasMaxLength(1000);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .IsRequired();

            // Indexes for better query performance
            builder.HasIndex(x => x.CreatedDate)
                .HasDatabaseName("idx_ExceptionLogs_CreatedDate");

            builder.HasIndex(x => x.ExceptionType)
                .HasDatabaseName("idx_ExceptionLogs_ExceptionType");

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("idx_ExceptionLogs_UserId");

            builder.HasIndex(x => x.RequestPath)
                .HasDatabaseName("idx_ExceptionLogs_RequestPath");
        }
    }
}
