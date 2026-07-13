using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class WorkflowActionConfiguration : IEntityTypeConfiguration<WorkflowAction>
{
    public void Configure(EntityTypeBuilder<WorkflowAction> entity)
    {
        entity.ToTable("workflow_actions");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ActionType).HasMaxLength(80).IsRequired();
        entity.Property(x => x.ConfigJson).HasColumnType("jsonb");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<Workflow>()
            .WithMany()
            .HasForeignKey(x => x.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.WorkflowId, x.Order });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
