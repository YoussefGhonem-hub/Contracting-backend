using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class LaborAttendanceRequestConfiguration : IEntityTypeConfiguration<LaborAttendanceRequest>
    {
        public void Configure(EntityTypeBuilder<LaborAttendanceRequest> builder)
        {
            builder.ToTable("LaborAttendanceRequests", "business");
            builder.Property(r => r.RequestNumber).HasMaxLength(50);
            builder.Property(r => r.SiteName).HasMaxLength(500);
            builder.Property(r => r.Notes).HasMaxLength(1000);

            builder.HasOne(r => r.Status)
                   .WithMany()
                   .HasForeignKey(r => r.StatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Project)
                   .WithMany()
                   .HasForeignKey(r => r.ProjectId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Supervisor)
                   .WithMany()
                   .HasForeignKey(r => r.SupervisorId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
