using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class LaborAttendanceActivityConfiguration : IEntityTypeConfiguration<LaborAttendanceActivity>
    {
        public void Configure(EntityTypeBuilder<LaborAttendanceActivity> builder)
        {
            builder.ToTable("LaborAttendanceActivities", "business");
            builder.Property(a => a.ActionType).HasMaxLength(100);
            builder.Property(a => a.Comments).HasMaxLength(1000);

            builder.HasOne(a => a.FromStatus)
                   .WithMany()
                   .HasForeignKey(a => a.FromStatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.ToStatus)
                   .WithMany()
                   .HasForeignKey(a => a.ToStatusId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
