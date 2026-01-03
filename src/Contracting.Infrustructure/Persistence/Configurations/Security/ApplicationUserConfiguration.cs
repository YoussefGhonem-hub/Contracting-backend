using Contracting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Security
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users", "security");
            builder.Property(u => u.FullName).HasMaxLength(200);
            builder.Property(u => u.AvatarUrl).HasMaxLength(500);
            builder.Property(u => u.IsActive).HasDefaultValue(true);
        }
    }
}
