using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class TaskCardConfiguration : IEntityTypeConfiguration<TaskCard>
{
    public void Configure(EntityTypeBuilder<TaskCard> entity)
    {
        entity.ToTable("task_cards");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Title).HasMaxLength(300).IsRequired();
        entity.Property(x => x.Priority).HasMaxLength(20).IsRequired();
        entity.Property(x => x.DueAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.CompletedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<TaskBoard>()
            .WithMany()
            .HasForeignKey(x => x.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne<TaskColumn>()
            .WithMany()
            .HasForeignKey(x => x.ColumnId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AssigneeUserId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne<Contact>()
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(x => new { x.ColumnId, x.SortOrder });
        entity.HasIndex(x => new { x.TenantId, x.BoardId });
        entity.HasIndex(x => new { x.TenantId, x.AssigneeUserId });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
