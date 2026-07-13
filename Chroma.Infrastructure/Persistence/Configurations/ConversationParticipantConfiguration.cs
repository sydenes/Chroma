using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> entity)
    {
        entity.ToTable("conversation_participants");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ExternalParticipantId).HasMaxLength(255);
        entity.Property(x => x.Role).HasMaxLength(40).IsRequired();

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne<Contact>()
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(x => new { x.ConversationId, x.UserId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"UserId\" IS NOT NULL");
        entity.HasIndex(x => new { x.ConversationId, x.ContactId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"ContactId\" IS NOT NULL");
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
