using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class ProjectScheduleConfiguration : IEntityTypeConfiguration<ProjectSchedule>
{
    public void Configure(EntityTypeBuilder<ProjectSchedule> builder)
    {
        builder.ToTable("ProjectSchedules", "client");
        builder.Property(s => s.Title).HasMaxLength(500);
        builder.Property(s => s.Version).HasMaxLength(100);
        builder.Property(s => s.FileName).HasMaxLength(500);
        builder.Property(s => s.Extension).HasMaxLength(20);
        builder.Property(s => s.Url).HasMaxLength(2000);

        builder.HasOne(s => s.Project)
            .WithMany()
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.UploadedByUser)
            .WithMany()
            .HasForeignKey(s => s.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
