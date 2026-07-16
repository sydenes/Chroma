using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("users");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.FirstName).HasMaxLength(120).IsRequired();
        entity.Property(x => x.LastName).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
        entity.Property(x => x.Phone).HasMaxLength(50);
        entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Avatar).HasMaxLength(500);
        entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
        entity.Property(x => x.LastLoginAtUtc).HasColumnType("timestamptz");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => x.Email).IsUnique().HasFilter("\"IsDeleted\" = false");
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
