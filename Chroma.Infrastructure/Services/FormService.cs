using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Forms.Dtos;
using Chroma.Application.Modules.Forms.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class FormService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IFormService
{
    public async Task<FormSearchResult> SearchAsync(FormSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Forms.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queryable = queryable.Where(x => x.Name.Contains(request.Query.Trim()));
        }

        if (request.IsActive.HasValue)
        {
            queryable = queryable.Where(x => x.IsActive == request.IsActive.Value);
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderBy(x => x.Name)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDtoWithoutFields())
            .ToListAsync(cancellationToken);

        return new FormSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<FormDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var form = await dbContext.Forms
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDtoWithoutFields())
            .FirstOrDefaultAsync(cancellationToken);

        if (form is null)
        {
            return null;
        }

        var fields = await dbContext.FormFields
            .AsNoTracking()
            .Where(x => x.FormId == id && x.TenantId == tenantId)
            .OrderBy(x => x.Order)
            .Select(MapFieldToDto())
            .ToListAsync(cancellationToken);

        return new FormDto
        {
            Id = form.Id,
            TenantId = form.TenantId,
            Name = form.Name,
            Description = form.Description,
            IsActive = form.IsActive,
            CreateContactOnSubmit = form.CreateContactOnSubmit,
            Fields = fields
        };
    }

    public async Task<FormDto> CreateAsync(CreateFormRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new Form
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = request.Description,
            CreateContactOnSubmit = request.CreateContactOnSubmit,
            IsActive = true
        };

        dbContext.Forms.Add(entity);

        foreach (var fieldRequest in request.Fields)
        {
            dbContext.FormFields.Add(new FormField
            {
                TenantId = tenantId,
                FormId = entity.Id,
                Name = fieldRequest.Name.Trim(),
                Label = fieldRequest.Label.Trim(),
                FieldType = fieldRequest.FieldType,
                IsRequired = fieldRequest.IsRequired,
                Order = fieldRequest.Order,
                OptionsJson = fieldRequest.OptionsJson
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<FormDto?> UpdateAsync(Guid id, UpdateFormRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Forms.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name.Trim();
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.CreateContactOnSubmit = request.CreateContactOnSubmit;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Forms.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        var fields = await dbContext.FormFields
            .Where(x => x.FormId == id && x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach (var field in fields)
        {
            field.IsDeleted = true;
            field.DeletedAtUtc = DateTime.UtcNow;
            field.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<FormFieldDto> CreateFieldAsync(Guid formId, CreateFormFieldRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var formExists = await dbContext.Forms.AsNoTracking()
            .AnyAsync(x => x.Id == formId && x.TenantId == tenantId, cancellationToken);

        if (!formExists)
        {
            throw new InvalidOperationException("Form not found.");
        }

        var entity = new FormField
        {
            TenantId = tenantId,
            FormId = formId,
            Name = request.Name.Trim(),
            Label = request.Label.Trim(),
            FieldType = request.FieldType,
            IsRequired = request.IsRequired,
            Order = request.Order,
            OptionsJson = request.OptionsJson
        };

        dbContext.FormFields.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToFieldDto(entity);
    }

    public async Task<FormFieldDto?> UpdateFieldAsync(Guid formId, Guid fieldId, UpdateFormFieldRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.FormFields
            .FirstOrDefaultAsync(x => x.Id == fieldId && x.FormId == formId && x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name.Trim();
        entity.Label = request.Label.Trim();
        entity.FieldType = request.FieldType;
        entity.IsRequired = request.IsRequired;
        entity.Order = request.Order;
        entity.OptionsJson = request.OptionsJson;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToFieldDto(entity);
    }

    public async Task<bool> DeleteFieldAsync(Guid formId, Guid fieldId, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.FormFields
            .FirstOrDefaultAsync(x => x.Id == fieldId && x.FormId == formId && x.TenantId == tenantId, cancellationToken);

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

    public async Task<FormResponseDto> SubmitResponseAsync(Guid formId, SubmitFormResponseRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var form = await dbContext.Forms
            .FirstOrDefaultAsync(x => x.Id == formId && x.TenantId == tenantId, cancellationToken)
            ?? throw new InvalidOperationException("Form not found.");

        Guid? contactId = null;

        if (form.CreateContactOnSubmit)
        {
            var firstName = string.IsNullOrWhiteSpace(request.ContactFirstName) ? "Unknown" : request.ContactFirstName.Trim();
            var lastName = string.IsNullOrWhiteSpace(request.ContactLastName) ? "Contact" : request.ContactLastName.Trim();

            var contact = new Contact
            {
                TenantId = tenantId,
                FirstName = firstName,
                LastName = lastName,
                Source = "form",
                Status = "active"
            };

            dbContext.Contacts.Add(contact);
            contactId = contact.Id;
        }

        var response = new FormResponse
        {
            TenantId = tenantId,
            FormId = formId,
            ContactId = contactId,
            JsonData = request.JsonData
        };

        dbContext.FormResponses.Add(response);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new FormResponseDto
        {
            Id = response.Id,
            TenantId = response.TenantId,
            FormId = response.FormId,
            ContactId = response.ContactId,
            JsonData = response.JsonData
        };
    }

    private static FormFieldDto ToFieldDto(FormField entity)
    {
        return new FormFieldDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            FormId = entity.FormId,
            Name = entity.Name,
            Label = entity.Label,
            FieldType = entity.FieldType,
            IsRequired = entity.IsRequired,
            Order = entity.Order,
            OptionsJson = entity.OptionsJson
        };
    }

    private static Expression<Func<FormField, FormFieldDto>> MapFieldToDto()
    {
        return x => new FormFieldDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            FormId = x.FormId,
            Name = x.Name,
            Label = x.Label,
            FieldType = x.FieldType,
            IsRequired = x.IsRequired,
            Order = x.Order,
            OptionsJson = x.OptionsJson
        };
    }

    private static Expression<Func<Form, FormDto>> MapToDtoWithoutFields()
    {
        return x => new FormDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
            CreateContactOnSubmit = x.CreateContactOnSubmit,
            Fields = new List<FormFieldDto>()
        };
    }
}
