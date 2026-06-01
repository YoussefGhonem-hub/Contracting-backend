using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class VariationOrderConfiguration : IEntityTypeConfiguration<VariationOrder>
{
    public void Configure(EntityTypeBuilder<VariationOrder> builder)
    {
        builder.ToTable("VariationOrders", "client");
        builder.Property(v => v.Title).HasMaxLength(500);
        builder.Property(v => v.Description).HasMaxLength(4000);
        builder.Property(v => v.Cost).HasColumnType("decimal(18,2)");
        builder.Property(v => v.ClientRejectionReason).HasMaxLength(2000);
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(v => v.ClientRejectionReason).HasMaxLength(2000);

        builder.HasOne(v => v.Project)
            .WithMany()
            .HasForeignKey(v => v.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.CreatedByEngineer)
            .WithMany()
            .HasForeignKey(v => v.CreatedByEngineerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
