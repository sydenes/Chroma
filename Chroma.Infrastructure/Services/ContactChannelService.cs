using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Contacts.Dtos;
using Chroma.Application.Modules.Contacts.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class ContactChannelService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IContactChannelService
{
    public async Task<ContactChannelSearchResult> SearchAsync(ContactChannelSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.ContactChannels
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ContactId == request.ContactId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var text = request.Query.Trim();
            queryable = queryable.Where(x => x.ChannelType.Contains(text) || x.Value.Contains(text));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderBy(x => x.ChannelType)
            .ThenBy(x => x.Value)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        return new ContactChannelSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<ContactChannelDto?> GetByIdAsync(Guid contactId, Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.ContactChannels
            .AsNoTracking()
            .Where(x => x.Id == id && x.ContactId == contactId && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ContactChannelDto> CreateAsync(CreateContactChannelRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var contactExists = await dbContext.Contacts
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ContactId && x.TenantId == tenantId, cancellationToken);

        if (!contactExists)
        {
            throw new InvalidOperationException("Contact not found.");
        }

        var entity = new ContactChannel
        {
            TenantId = tenantId,
            ContactId = request.ContactId,
            ChannelType = request.ChannelType.Trim(),
            Value = request.Value.Trim(),
            IsPrimary = request.IsPrimary
        };

        dbContext.ContactChannels.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<ContactChannelDto?> UpdateAsync(Guid contactId, Guid id, UpdateContactChannelRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.ContactChannels
            .FirstOrDefaultAsync(x => x.Id == id && x.ContactId == contactId && x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.ChannelType = request.ChannelType.Trim();
        entity.Value = request.Value.Trim();
        entity.IsPrimary = request.IsPrimary;
        entity.Verified = request.Verified;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid contactId, Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.ContactChannels
            .FirstOrDefaultAsync(x => x.Id == id && x.ContactId == contactId && x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ContactChannelDto ToDto(ContactChannel entity)
    {
        return new ContactChannelDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            ContactId = entity.ContactId,
            ChannelType = entity.ChannelType,
            Value = entity.Value,
            IsPrimary = entity.IsPrimary,
            Verified = entity.Verified
        };
    }

    private static Expression<Func<ContactChannel, ContactChannelDto>> MapToDto()
    {
        return x => new ContactChannelDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            ContactId = x.ContactId,
            ChannelType = x.ChannelType,
            Value = x.Value,
            IsPrimary = x.IsPrimary,
            Verified = x.Verified
        };
    }
}
