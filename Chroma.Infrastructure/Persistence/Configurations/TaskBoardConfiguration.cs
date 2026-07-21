using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class TaskBoardConfiguration : IEntityTypeConfiguration<TaskBoard>
{
    public void Configure(EntityTypeBuilder<TaskBoard> entity)
    {
        entity.ToTable("task_boards");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => new { x.TenantId, x.IsDefault });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
