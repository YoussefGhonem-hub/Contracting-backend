using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class PurchaseRequestReceiptConfiguration : IEntityTypeConfiguration<PurchaseRequestReceipt>
    {
        public void Configure(EntityTypeBuilder<PurchaseRequestReceipt> builder)
        {
            builder.ToTable("PurchaseRequestReceipts", "business");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasOne(x => x.EngineerRequest)
                .WithMany(r => r.PurchaseReceipts)
                .HasForeignKey(x => x.EngineerRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ReceivedBy)
                .WithMany()
                .HasForeignKey(x => x.ReceivedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
