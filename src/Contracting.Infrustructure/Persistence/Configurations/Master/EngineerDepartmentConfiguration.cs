using Contracting.Domain.Entities.master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Master
{
    public class EngineerDepartmentConfiguration : IEntityTypeConfiguration<EngineerDepartment>
    {
        public void Configure(EntityTypeBuilder<EngineerDepartment> builder)
        {
            builder.ToTable("EngineerDepartments", "master");

            builder.HasKey(ed => ed.Id);

            builder.HasOne(ed => ed.Engineer)
                .WithMany(e => e.EngineerDepartments)
                .HasForeignKey(ed => ed.EngineerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ed => ed.Department)
                .WithMany(d => d.EngineerDepartments)
                .HasForeignKey(ed => ed.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ed => ed.Role)
                .WithMany()
                .HasForeignKey(ed => ed.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ed => new { ed.EngineerId, ed.DepartmentId }).IsUnique();
        }
    }
}
