using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Contacts.Dtos;
using Chroma.Application.Modules.Contacts.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class ContactService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IContactService
{
    public async Task<ContactSearchResult> SearchAsync(ContactSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Contacts.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var text = request.Query.Trim();
            queryable = queryable.Where(x => (x.FirstName + " " + x.LastName).Contains(text));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var contacts = await queryable
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        return new ContactSearchResult
        {
            TotalCount = totalCount,
            Items = contacts
        };
    }

    public async Task<ContactDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.Contacts
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ContactDto> CreateAsync(CreateContactRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new Contact
        {
            TenantId = tenantId,
            OwnerId = request.OwnerId,
            CompanyId = request.CompanyId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Source = request.Source,
            Status = "active"
        };

        dbContext.Contacts.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<ContactDto?> UpdateAsync(Guid id, UpdateContactRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.CompanyId = request.CompanyId;
        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Source = request.Source;
        entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToLowerInvariant();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
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

    private static ContactDto ToDto(Contact entity)
    {
        return new ContactDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Status = entity.Status,
            Source = entity.Source
        };
    }

    private static Expression<Func<Contact, ContactDto>> MapToDto()
    {
        return x => new ContactDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            CompanyId = x.CompanyId,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Status = x.Status,
            Source = x.Source
        };
    }
}
