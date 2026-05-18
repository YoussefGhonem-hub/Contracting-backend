using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class LaborAttendanceRecordConfiguration : IEntityTypeConfiguration<LaborAttendanceRecord>
    {
        public void Configure(EntityTypeBuilder<LaborAttendanceRecord> builder)
        {
            builder.ToTable("LaborAttendanceRecords", "business");
            builder.Property(r => r.Name).HasMaxLength(300);
            builder.Property(r => r.JobTitle).HasMaxLength(300);
            builder.Property(r => r.AttendanceStatus).HasConversion<string>();
            builder.Property(r => r.DailyRate).HasColumnType("decimal(18,2)");
            builder.Property(r => r.OvertimeHours).HasColumnType("decimal(18,2)");
            builder.Property(r => r.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Property(r => r.Notes).HasMaxLength(1000);
        }
    }
}
