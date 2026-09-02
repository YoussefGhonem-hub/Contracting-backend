using Contracting.Domain.Entities.master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Master
{
    public class RequestTypeDefaultDepartmentConfiguration : IEntityTypeConfiguration<RequestTypeDefaultDepartment>
    {
        public void Configure(EntityTypeBuilder<RequestTypeDefaultDepartment> builder)
        {
            builder.ToTable("RequestTypeDefaultDepartments", "master");

            builder.Property(x => x.RequestType).HasMaxLength(50).IsRequired();

            // One default department per (branch, request type) — filtered to active rows only,
            // so a soft-deleted config (IsDeleted = true) doesn't permanently block recreating one
            // for the same branch/request type.
            builder.HasIndex(x => new { x.BranchId, x.RequestType })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
