using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.ToTable("audit_logs");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Entity).HasMaxLength(80).IsRequired();
        entity.Property(x => x.Action).HasMaxLength(40).IsRequired();
        entity.Property(x => x.OldValueJson).HasColumnType("jsonb");
        entity.Property(x => x.NewValueJson).HasColumnType("jsonb");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(x => new { x.TenantId, x.Entity, x.EntityId, x.CreatedAtUtc });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
