using Contracting.Domain.Entities.business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Business
{
    public class EngineerRequestConfiguration : IEntityTypeConfiguration<EngineerRequest>
    {
        public void Configure(EntityTypeBuilder<EngineerRequest> builder)
        {
            builder.ToTable("EngineerRequests", "business");
            builder.Property(r => r.RequestTitle).HasMaxLength(200);
            builder.Property(r => r.Descreption).HasMaxLength(500);
        }
    }
}
