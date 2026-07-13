using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> entity)
    {
        entity.ToTable("conversations");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
        entity.Property(x => x.ExternalConversationId).HasMaxLength(255);
        entity.Property(x => x.LastMessageAtUtc).HasColumnType("timestamptz");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<Channel>()
            .WithMany()
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<Contact>()
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(x => new { x.TenantId, x.ChannelId, x.Status });
        entity.HasIndex(x => new { x.TenantId, x.AssignedUserId });
        entity.HasIndex(x => new { x.ChannelId, x.ExternalConversationId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"ExternalConversationId\" IS NOT NULL");
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
