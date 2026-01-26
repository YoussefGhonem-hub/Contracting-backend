using Contracting.Domain.Entities;
using Contracting.Domain.Entities.helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Security;

public class UserSignatureConfiguration : IEntityTypeConfiguration<UserSignature>
{
    public void Configure(EntityTypeBuilder<UserSignature> builder)
    {
        builder.ToTable("UserSignatures", "security");

        builder.Property(x => x.SignatureUrl)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne(u => u.Signature)
            .HasForeignKey<UserSignature>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
