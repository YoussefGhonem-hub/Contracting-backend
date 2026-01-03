using Contracting.Domain.Entities.helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Security
{
    public class UserDeviceTokenConfiguration : IEntityTypeConfiguration<UserDeviceToken>
    {
        public void Configure(EntityTypeBuilder<UserDeviceToken> builder)
        {
            builder.ToTable("UserDeviceTokens", "security");
            builder.Property(t => t.FcmToken).IsRequired().HasMaxLength(500);
            builder.HasIndex(t => new { t.UserId, t.FcmToken }).IsUnique();
        }
    }
}
