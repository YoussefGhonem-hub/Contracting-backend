using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class TransferRequestConfiguration : IEntityTypeConfiguration<TransferRequest>
    {
        public void Configure(EntityTypeBuilder<TransferRequest> builder)
        {
            builder.ToTable("TransferRequests", "business");
            builder.Property(r => r.RequestNumber).HasMaxLength(50);
            builder.Property(r => r.SourceWarehouse).HasMaxLength(500);
            builder.Property(r => r.DestinationWarehouse).HasMaxLength(500);
            builder.Property(r => r.Notes).HasMaxLength(1000);

            builder.HasOne(r => r.Status)
                   .WithMany()
                   .HasForeignKey(r => r.StatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.SourceProject)
                   .WithMany()
                   .HasForeignKey(r => r.SourceProjectId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.DestinationProject)
                   .WithMany()
                   .HasForeignKey(r => r.DestinationProjectId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.RequestedBy)
                   .WithMany()
                   .HasForeignKey(r => r.RequestedById)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(r => r.NeedsAcknowledgment).HasDefaultValue(false);
        }
    }
}
