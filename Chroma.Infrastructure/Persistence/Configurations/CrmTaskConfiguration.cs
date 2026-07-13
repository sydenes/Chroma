using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class CrmTaskConfiguration : IEntityTypeConfiguration<CrmTask>
{
    public void Configure(EntityTypeBuilder<CrmTask> entity)
    {
        entity.ToTable("tasks");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Priority).HasMaxLength(20).IsRequired();
        entity.Property(x => x.DueAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.CompletedAtUtc).HasColumnType("timestamptz");

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

        entity.HasIndex(x => new { x.TenantId, x.OwnerId });
        entity.HasIndex(x => new { x.TenantId, x.Status, x.DueAtUtc });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
