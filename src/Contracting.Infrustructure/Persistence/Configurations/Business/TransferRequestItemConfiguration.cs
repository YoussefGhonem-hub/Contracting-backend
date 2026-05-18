using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class TransferRequestItemConfiguration : IEntityTypeConfiguration<TransferRequestItem>
    {
        public void Configure(EntityTypeBuilder<TransferRequestItem> builder)
        {
            builder.ToTable("TransferRequestItems", "business");
            builder.Property(i => i.ItemCode).HasMaxLength(100);
            builder.Property(i => i.ItemName).HasMaxLength(500);
            builder.Property(i => i.Unit).HasMaxLength(100);
            builder.Property(i => i.Quantity).HasColumnType("decimal(18,4)");
            builder.Property(i => i.Notes).HasMaxLength(1000);
        }
    }
}
