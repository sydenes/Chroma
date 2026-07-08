using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Contact> Contacts { get; }
    DbSet<ContactChannel> ContactChannels { get; }
    DbSet<Company> Companies { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
