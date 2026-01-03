using Contracting.Domain.Entities.master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Master
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments", "master");
            builder.Property(d => d.nameEn).HasMaxLength(200);
            builder.Property(d => d.nameAr).HasMaxLength(200);
            builder.HasOne(d => d.Branch).WithMany(b => b.Departments).HasForeignKey(d => d.BranchId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
