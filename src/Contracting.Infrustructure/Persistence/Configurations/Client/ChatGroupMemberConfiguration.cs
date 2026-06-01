using Contracting.Domain.Entities.client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class ChatGroupMemberConfiguration : IEntityTypeConfiguration<ChatGroupMember>
{
    public void Configure(EntityTypeBuilder<ChatGroupMember> builder)
    {
        builder.ToTable("ChatGroupMembers", "client");
        builder.Property(m => m.MemberType).HasMaxLength(50).IsRequired();

        builder.HasOne(m => m.ChatGroup)
            .WithMany(g => g.Members)
            .HasForeignKey(m => m.ChatGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.ApplicationUser)
            .WithMany()
            .HasForeignKey(m => m.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
