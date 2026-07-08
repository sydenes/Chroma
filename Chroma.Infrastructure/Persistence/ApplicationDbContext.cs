using Chroma.Application.Abstractions;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ContactChannel> ContactChannels => Set<ContactChannel>();
    public DbSet<Company> Companies => Set<Company>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("contacts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(80);
            entity.HasIndex(x => new { x.TenantId, x.LastName, x.FirstName });
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ContactChannel>(entity =>
        {
            entity.ToTable("contact_channels");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ChannelType).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(255).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.ChannelType, x.Value }).IsUnique();
            entity.HasOne<Contact>().WithMany().HasForeignKey(x => x.ContactId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.Website).HasMaxLength(255);
            entity.HasIndex(x => new { x.TenantId, x.Name });
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        base.OnModelCreating(modelBuilder);
    }
}
