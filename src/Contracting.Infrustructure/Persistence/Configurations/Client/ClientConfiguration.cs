using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class ClientConfiguration : IEntityTypeConfiguration<Contracting.Domain.Entities.client.Client>
{
    public void Configure(EntityTypeBuilder<Contracting.Domain.Entities.client.Client> builder)
    {
        builder.ToTable("Clients", "client");
        builder.Property(c => c.CompanyName).HasMaxLength(300);
        builder.Property(c => c.PhoneNumber).HasMaxLength(50);
        builder.Property(c => c.Address).HasMaxLength(500);

        builder.HasOne(c => c.ApplicationUser)
            .WithMany()
            .HasForeignKey(c => c.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
