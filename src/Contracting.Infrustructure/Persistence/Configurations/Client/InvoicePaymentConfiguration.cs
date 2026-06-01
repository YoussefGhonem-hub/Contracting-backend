using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class InvoicePaymentConfiguration : IEntityTypeConfiguration<InvoicePayment>
{
    public void Configure(EntityTypeBuilder<InvoicePayment> builder)
    {
        builder.ToTable("InvoicePayments", "client");
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Reference).HasMaxLength(300);
        builder.Property(p => p.Notes).HasMaxLength(2000);

        builder.HasOne(p => p.ProjectInvoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.ProjectInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
