using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class FinancialClearanceItemConfiguration : IEntityTypeConfiguration<FinancialClearanceItem>
    {
        public void Configure(EntityTypeBuilder<FinancialClearanceItem> builder)
        {
            builder.ToTable("FinancialClearanceItems", "business");
            builder.Property(i => i.ItemName).HasMaxLength(500).IsRequired();
            builder.Property(i => i.Value).HasColumnType("decimal(18,2)");

            builder.HasOne(i => i.FinancialClearance)
                   .WithMany(c => c.Items)
                   .HasForeignKey(i => i.FinancialClearanceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
