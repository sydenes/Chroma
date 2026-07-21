using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class TaskColumnConfiguration : IEntityTypeConfiguration<TaskColumn>
{
    public void Configure(EntityTypeBuilder<TaskColumn> entity)
    {
        entity.ToTable("task_columns");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Color).HasMaxLength(40);
        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<TaskBoard>()
            .WithMany()
            .HasForeignKey(x => x.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.BoardId, x.SortOrder });
        entity.HasIndex(x => x.TenantId);
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
