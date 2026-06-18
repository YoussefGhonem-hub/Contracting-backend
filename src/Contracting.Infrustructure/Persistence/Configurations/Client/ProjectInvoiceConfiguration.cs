using Contracting.Domain.Common.Enums;
using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class ProjectInvoiceConfiguration : IEntityTypeConfiguration<ProjectInvoice>
{
    public void Configure(EntityTypeBuilder<ProjectInvoice> builder)
    {
        builder.ToTable("ProjectInvoices", "client");
        builder.Property(i => i.Title).HasMaxLength(500);
        builder.Property(i => i.TotalValue).HasColumnType("decimal(18,2)");
        builder.Property(i => i.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.Status).HasConversion(new PaymentStatusConverter()).HasMaxLength(50);
        builder.Property(i => i.Notes).HasMaxLength(2000);

        builder.HasOne(i => i.Project)
            .WithMany()
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.UpdatedByUser)
            .WithMany()
            .HasForeignKey(i => i.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
