using Chroma.Application.Abstractions;
using Chroma.Application.Modules.CustomFields.Dtos;
using Chroma.Application.Modules.CustomFields.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class CustomFieldService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : ICustomFieldService
{
    public async Task<CustomFieldSearchResult> SearchAsync(CustomFieldSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.CustomFields.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            queryable = queryable.Where(x => x.EntityType == request.EntityType.Trim());
        }

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

        return new CustomFieldSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<CustomFieldDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.CustomFields
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CustomFieldDto> CreateAsync(CreateCustomFieldRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new CustomField
        {
            TenantId = tenantId,
            EntityType = request.EntityType.Trim(),
            Name = request.Name.Trim(),
            FieldType = request.FieldType,
            SettingsJson = request.SettingsJson
        };

        dbContext.CustomFields.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<CustomFieldDto?> UpdateAsync(Guid id, UpdateCustomFieldRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.CustomFields.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name.Trim();
        entity.FieldType = request.FieldType;
        entity.SettingsJson = request.SettingsJson;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.CustomFields.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
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

    public async Task<IReadOnlyCollection<CustomFieldValueDto>> GetValuesAsync(GetCustomFieldValuesRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.CustomFieldValues
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityId == request.EntityId)
            .Join(
                dbContext.CustomFields.AsNoTracking().Where(f => f.EntityType == request.EntityType.Trim() && f.TenantId == tenantId),
                value => value.FieldId,
                field => field.Id,
                (value, _) => value)
            .Select(MapValueToDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomFieldValueDto> SetValueAsync(SetCustomFieldValueRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var fieldExists = await dbContext.CustomFields.AsNoTracking()
            .AnyAsync(x => x.Id == request.FieldId && x.TenantId == tenantId, cancellationToken);

        if (!fieldExists)
        {
            throw new InvalidOperationException("Custom field not found.");
        }

        var entity = await dbContext.CustomFieldValues
            .FirstOrDefaultAsync(x => x.FieldId == request.FieldId && x.EntityId == request.EntityId && x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            entity = new CustomFieldValue
            {
                TenantId = tenantId,
                FieldId = request.FieldId,
                EntityId = request.EntityId,
                Value = request.Value
            };

            dbContext.CustomFieldValues.Add(entity);
        }
        else
        {
            entity.Value = request.Value;
            entity.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToValueDto(entity);
    }

    private static CustomFieldDto ToDto(CustomField entity)
    {
        return new CustomFieldDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            EntityType = entity.EntityType,
            Name = entity.Name,
            FieldType = entity.FieldType,
            SettingsJson = entity.SettingsJson
        };
    }

    private static CustomFieldValueDto ToValueDto(CustomFieldValue entity)
    {
        return new CustomFieldValueDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            FieldId = entity.FieldId,
            EntityId = entity.EntityId,
            Value = entity.Value
        };
    }

    private static Expression<Func<CustomField, CustomFieldDto>> MapToDto()
    {
        return x => new CustomFieldDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            EntityType = x.EntityType,
            Name = x.Name,
            FieldType = x.FieldType,
            SettingsJson = x.SettingsJson
        };
    }

    private static Expression<Func<CustomFieldValue, CustomFieldValueDto>> MapValueToDto()
    {
        return x => new CustomFieldValueDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            FieldId = x.FieldId,
            EntityId = x.EntityId,
            Value = x.Value
        };
    }
}
