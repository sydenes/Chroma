using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
{
    public void Configure(EntityTypeBuilder<TaskComment> entity)
    {
        entity.ToTable("task_comments");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Body).IsRequired();
        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<TaskCard>()
            .WithMany()
            .HasForeignKey(x => x.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.CardId, x.CreatedAtUtc });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
