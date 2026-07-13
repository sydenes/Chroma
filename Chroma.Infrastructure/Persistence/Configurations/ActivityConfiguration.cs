using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> entity)
    {
        entity.ToTable("activities");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ActivityType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        entity.Property(x => x.OccurredAtUtc).HasColumnType("timestamptz");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne<Contact>()
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne<Deal>()
            .WithMany()
            .HasForeignKey(x => x.DealId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(x => new { x.TenantId, x.ContactId });
        entity.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
