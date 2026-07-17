using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> entity)
    {
        entity.ToTable("messages");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Direction).HasMaxLength(10).IsRequired();
        entity.Property(x => x.SenderDisplayName).HasMaxLength(200);
        entity.Property(x => x.MessageType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.ExternalId).HasMaxLength(255);
        entity.Property(x => x.MediaUrl).HasMaxLength(1000);
        entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
        entity.Property(x => x.SentAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeliveredAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.ReadAtUtc).HasColumnType("timestamptz");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne<Channel>()
            .WithMany()
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.SenderUserId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne<StoredFile>()
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(x => new { x.ConversationId, x.SentAtUtc });
        entity.HasIndex(x => x.SenderUserId);
        entity.HasIndex(x => x.FileId);
        entity.HasIndex(x => new { x.ChannelId, x.ExternalId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"ExternalId\" IS NOT NULL");
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
