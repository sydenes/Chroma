using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Tags.Dtos;
using Chroma.Application.Modules.Tags.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class TagService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : ITagService
{
    public async Task<TagSearchResult> SearchAsync(TagSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Tags.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queryable = queryable.Where(x => x.Name.Contains(request.Query.Trim()));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderBy(x => x.Name)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        return new TagSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<TagDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.Tags
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TagDto> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new Tag
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Color = request.Color
        };

        dbContext.Tags.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<TagDto?> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Tags.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name.Trim();
        entity.Color = request.Color;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Tags.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
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

    public async Task<bool> AssignToContactAsync(Guid tagId, Guid contactId, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var tagExists = await dbContext.Tags.AsNoTracking()
            .AnyAsync(x => x.Id == tagId && x.TenantId == tenantId, cancellationToken);
        var contactExists = await dbContext.Contacts.AsNoTracking()
            .AnyAsync(x => x.Id == contactId && x.TenantId == tenantId, cancellationToken);

        if (!tagExists || !contactExists)
        {
            return false;
        }

        var exists = await dbContext.ContactTags
            .AnyAsync(x => x.ContactId == contactId && x.TagId == tagId, cancellationToken);

        if (exists)
        {
            return true;
        }

        dbContext.ContactTags.Add(new ContactTag { ContactId = contactId, TagId = tagId });
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AssignToCompanyAsync(Guid tagId, Guid companyId, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var tagExists = await dbContext.Tags.AsNoTracking()
            .AnyAsync(x => x.Id == tagId && x.TenantId == tenantId, cancellationToken);
        var companyExists = await dbContext.Companies.AsNoTracking()
            .AnyAsync(x => x.Id == companyId && x.TenantId == tenantId, cancellationToken);

        if (!tagExists || !companyExists)
        {
            return false;
        }

        var exists = await dbContext.CompanyTags
            .AnyAsync(x => x.CompanyId == companyId && x.TagId == tagId, cancellationToken);

        if (exists)
        {
            return true;
        }

        dbContext.CompanyTags.Add(new CompanyTag { CompanyId = companyId, TagId = tagId });
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnassignFromContactAsync(Guid tagId, Guid contactId, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var tagExists = await dbContext.Tags.AsNoTracking()
            .AnyAsync(x => x.Id == tagId && x.TenantId == tenantId, cancellationToken);

        if (!tagExists)
        {
            return false;
        }

        var link = await dbContext.ContactTags
            .FirstOrDefaultAsync(x => x.ContactId == contactId && x.TagId == tagId, cancellationToken);

        if (link is null)
        {
            return false;
        }

        dbContext.ContactTags.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnassignFromCompanyAsync(Guid tagId, Guid companyId, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var tagExists = await dbContext.Tags.AsNoTracking()
            .AnyAsync(x => x.Id == tagId && x.TenantId == tenantId, cancellationToken);

        if (!tagExists)
        {
            return false;
        }

        var link = await dbContext.CompanyTags
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.TagId == tagId, cancellationToken);

        if (link is null)
        {
            return false;
        }

        dbContext.CompanyTags.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static TagDto ToDto(Tag entity)
    {
        return new TagDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Color = entity.Color
        };
    }

    private static Expression<Func<Tag, TagDto>> MapToDto()
    {
        return x => new TagDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            Name = x.Name,
            Color = x.Color
        };
    }
}
