using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class FinancialClearanceConfiguration : IEntityTypeConfiguration<FinancialClearance>
    {
        public void Configure(EntityTypeBuilder<FinancialClearance> builder)
        {
            builder.ToTable("FinancialClearances", "business");
            builder.Property(r => r.ClearanceNumber).HasMaxLength(50);
            builder.Property(r => r.EmployeeName).HasMaxLength(300);
            builder.Property(r => r.Notes).HasMaxLength(1000);
            builder.Property(r => r.Status).HasConversion<string>();
            builder.Property(r => r.AdvanceAmount).HasColumnType("decimal(18,2)");
            builder.Property(r => r.SpentAmount).HasColumnType("decimal(18,2)");
            builder.Property(r => r.RemainingAmount).HasColumnType("decimal(18,2)");

            builder.HasOne(r => r.RequestedBy)
                   .WithMany()
                   .HasForeignKey(r => r.RequestedById)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Department)
                   .WithMany()
                   .HasForeignKey(r => r.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Project)
                   .WithMany()
                   .HasForeignKey(r => r.ProjectId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
