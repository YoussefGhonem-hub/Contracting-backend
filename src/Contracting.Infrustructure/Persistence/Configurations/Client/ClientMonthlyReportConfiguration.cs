using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class ClientMonthlyReportConfiguration : IEntityTypeConfiguration<ClientMonthlyReport>
{
    public void Configure(EntityTypeBuilder<ClientMonthlyReport> builder)
    {
        builder.ToTable("ClientMonthlyReports", "client");
        builder.Property(r => r.Title).HasMaxLength(500);
        builder.Property(r => r.WorkProgress).HasMaxLength(4000);

        builder.HasOne(r => r.Project)
            .WithMany()
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.UploadedByUser)
            .WithMany()
            .HasForeignKey(r => r.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
