using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> entity)
    {
        entity.ToTable("notifications");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Body).IsRequired();
        entity.Property(x => x.NotificationType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.SourceType).HasMaxLength(40);
        entity.Property(x => x.ReadAtUtc).HasColumnType("timestamptz");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.TenantId, x.UserId, x.IsRead });
        entity.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        entity.HasIndex(x => new { x.UserId, x.SourceType, x.SourceId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"SourceType\" IS NOT NULL AND \"SourceId\" IS NOT NULL");
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
