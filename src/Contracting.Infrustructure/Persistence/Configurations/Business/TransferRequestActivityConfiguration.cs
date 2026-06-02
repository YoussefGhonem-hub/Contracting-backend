using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class TransferRequestActivityConfiguration : IEntityTypeConfiguration<TransferRequestActivity>
    {
        public void Configure(EntityTypeBuilder<TransferRequestActivity> builder)
        {
            builder.ToTable("TransferRequestActivities", "business");
            builder.Property(a => a.ActionType).HasMaxLength(100);
            builder.Property(a => a.Comments).HasMaxLength(1000);
            builder.Property(a => a.FromStatus).HasConversion<string>();
            builder.Property(a => a.ToStatus).HasConversion<string>();
        }
    }
}
