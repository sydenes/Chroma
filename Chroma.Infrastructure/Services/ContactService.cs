using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Contacts.Dtos;
using Chroma.Application.Modules.Contacts.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class ContactService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser) : IContactService
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

        if (!string.IsNullOrWhiteSpace(request.PotentialType))
        {
            queryable = queryable.Where(x => x.PotentialType == request.PotentialType.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(request.LifecycleStage))
        {
            queryable = queryable.Where(x => x.LifecycleStage == request.LifecycleStage.Trim().ToLowerInvariant());
        }

        if (request.OwnerId.HasValue)
        {
            queryable = queryable.Where(x => x.OwnerId == request.OwnerId.Value);
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

        await PopulatePrimaryChannelsAsync(contacts, cancellationToken);
        await PopulateOwnersAsync(contacts, cancellationToken);

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

        var contact = await dbContext.Contacts
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (contact is not null)
        {
            await PopulatePrimaryChannelsAsync([contact], cancellationToken);
            await PopulateOwnersAsync([contact], cancellationToken);
        }

        return contact;
    }

    public async Task<ContactDto> CreateAsync(CreateContactRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        await EnsureChannelValueAvailableAsync(tenantId, Guid.Empty, "phone", request.Phone, cancellationToken);
        await EnsureChannelValueAvailableAsync(tenantId, Guid.Empty, "email", request.Email, cancellationToken);

        var ownerId = await ResolveOwnerIdAsync(tenantId, request.OwnerId, cancellationToken);

        var entity = new Contact
        {
            TenantId = tenantId,
            OwnerId = ownerId,
            CompanyId = request.CompanyId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            JobTitle = Clean(request.JobTitle),
            Description = Clean(request.Description),
            Source = Clean(request.Source),
            PotentialType = NormalizeCode(request.PotentialType, "lead"),
            LifecycleStage = NormalizeCode(request.LifecycleStage, "new"),
            EstimatedValue = request.EstimatedValue,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "TRY" : request.Currency.Trim().ToUpperInvariant(),
            Status = "active"
        };

        dbContext.Contacts.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await UpsertChannelAsync(tenantId, entity.Id, "phone", request.Phone, true, cancellationToken);
        await UpsertChannelAsync(tenantId, entity.Id, "email", request.Email, false, cancellationToken);

        var dto = ToDto(entity);
        dto.Phone = Clean(request.Phone);
        dto.Email = Clean(request.Email);
        await PopulateOwnersAsync([dto], cancellationToken);
        return dto;
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

        await EnsureChannelValueAvailableAsync(tenantId, entity.Id, "phone", request.Phone, cancellationToken);
        await EnsureChannelValueAvailableAsync(tenantId, entity.Id, "email", request.Email, cancellationToken);

        entity.CompanyId = request.CompanyId;
        entity.OwnerId = await ResolveOwnerIdAsync(tenantId, request.OwnerId, cancellationToken);
        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.JobTitle = Clean(request.JobTitle);
        entity.Description = Clean(request.Description);
        entity.Source = Clean(request.Source);
        entity.PotentialType = NormalizeCode(request.PotentialType, entity.PotentialType);
        entity.LifecycleStage = NormalizeCode(request.LifecycleStage, entity.LifecycleStage);
        entity.EstimatedValue = request.EstimatedValue;
        entity.Currency = string.IsNullOrWhiteSpace(request.Currency)
            ? entity.Currency
            : request.Currency.Trim().ToUpperInvariant();
        entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToLowerInvariant();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await UpsertChannelAsync(tenantId, entity.Id, "phone", request.Phone, true, cancellationToken);
        await UpsertChannelAsync(tenantId, entity.Id, "email", request.Email, false, cancellationToken);

        var dto = ToDto(entity);
        dto.Phone = Clean(request.Phone);
        dto.Email = Clean(request.Email);
        await PopulateOwnersAsync([dto], cancellationToken);
        return dto;
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

    private async Task<Guid?> ResolveOwnerIdAsync(
        Guid tenantId,
        Guid? requestedOwnerId,
        CancellationToken cancellationToken)
    {
        var ownerId = requestedOwnerId ?? currentUser.UserId;
        if (ownerId is null)
        {
            return null;
        }

        var isMember = await dbContext.UserTenants
            .AsNoTracking()
            .AnyAsync(x => x.UserId == ownerId && x.TenantId == tenantId, cancellationToken);

        if (!isMember)
        {
            throw new InvalidOperationException("Seçilen kullanıcı bu müşteriye atanamaz.");
        }

        return ownerId;
    }

    private async Task PopulateOwnersAsync(
        IReadOnlyCollection<ContactDto> contacts,
        CancellationToken cancellationToken)
    {
        if (contacts.Count == 0)
        {
            return;
        }

        var ownerIds = contacts
            .Where(x => x.OwnerId.HasValue)
            .Select(x => x.OwnerId!.Value)
            .Distinct()
            .ToArray();

        if (ownerIds.Length == 0)
        {
            return;
        }

        var owners = await dbContext.Users
            .AsNoTracking()
            .Where(x => ownerIds.Contains(x.Id))
            .Select(x => new { x.Id, Name = x.FirstName + " " + x.LastName })
            .ToListAsync(cancellationToken);

        var nameById = owners.ToDictionary(x => x.Id, x => x.Name.Trim());

        foreach (var contact in contacts)
        {
            if (contact.OwnerId is Guid ownerId && nameById.TryGetValue(ownerId, out var name))
            {
                contact.OwnerName = name;
            }
        }
    }

    private async Task PopulatePrimaryChannelsAsync(
        IReadOnlyCollection<ContactDto> contacts,
        CancellationToken cancellationToken)
    {
        if (contacts.Count == 0)
        {
            return;
        }

        var contactIds = contacts.Select(x => x.Id).ToArray();
        var channels = await dbContext.ContactChannels
            .AsNoTracking()
            .Where(x => contactIds.Contains(x.ContactId) && (x.ChannelType == "phone" || x.ChannelType == "email"))
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var phoneByContact = channels
            .Where(x => x.ChannelType == "phone")
            .GroupBy(x => x.ContactId)
            .ToDictionary(x => x.Key, x => x.First().Value);
        var emailByContact = channels
            .Where(x => x.ChannelType == "email")
            .GroupBy(x => x.ContactId)
            .ToDictionary(x => x.Key, x => x.First().Value);

        foreach (var contact in contacts)
        {
            contact.Phone = phoneByContact.GetValueOrDefault(contact.Id);
            contact.Email = emailByContact.GetValueOrDefault(contact.Id);
        }
    }

    private async Task UpsertChannelAsync(
        Guid tenantId,
        Guid contactId,
        string channelType,
        string? value,
        bool isPrimary,
        CancellationToken cancellationToken)
    {
        value = Clean(value);
        var existing = await dbContext.ContactChannels
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.ContactId == contactId && x.ChannelType == channelType,
                cancellationToken);

        if (string.IsNullOrWhiteSpace(value))
        {
            if (existing is not null && !existing.IsDeleted)
            {
                existing.IsDeleted = true;
                existing.DeletedAtUtc = DateTime.UtcNow;
                existing.UpdatedAtUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        await EnsureChannelValueAvailableAsync(tenantId, contactId, channelType, value, cancellationToken);

        if (existing is null)
        {
            dbContext.ContactChannels.Add(new ContactChannel
            {
                TenantId = tenantId,
                ContactId = contactId,
                ChannelType = channelType,
                Value = value,
                IsPrimary = isPrimary
            });
        }
        else
        {
            existing.IsDeleted = false;
            existing.DeletedAtUtc = null;
            existing.Value = value;
            existing.IsPrimary = isPrimary;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureChannelValueAvailableAsync(
        Guid tenantId,
        Guid contactId,
        string channelType,
        string? value,
        CancellationToken cancellationToken)
    {
        value = Clean(value);
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var exists = await dbContext.ContactChannels
            .IgnoreQueryFilters()
            .AnyAsync(
                x => x.TenantId == tenantId &&
                    x.ChannelType == channelType &&
                    x.Value == value &&
                    x.ContactId != contactId &&
                    !x.IsDeleted,
                cancellationToken);

        if (!exists)
        {
            return;
        }

        var label = channelType == "email" ? "E-posta" : "Telefon";
        throw new InvalidOperationException($"{label} başka bir potansiyel kaydında kullanılıyor.");
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeCode(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim().ToLowerInvariant();
    }

    private static ContactDto ToDto(Contact entity)
    {
        return new ContactDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            OwnerId = entity.OwnerId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            JobTitle = entity.JobTitle,
            Description = entity.Description,
            Status = entity.Status,
            Source = entity.Source,
            PotentialType = entity.PotentialType,
            LifecycleStage = entity.LifecycleStage,
            EstimatedValue = entity.EstimatedValue,
            Currency = entity.Currency,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    private static Expression<Func<Contact, ContactDto>> MapToDto()
    {
        return x => new ContactDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            CompanyId = x.CompanyId,
            OwnerId = x.OwnerId,
            FirstName = x.FirstName,
            LastName = x.LastName,
            JobTitle = x.JobTitle,
            Description = x.Description,
            Status = x.Status,
            Source = x.Source,
            PotentialType = x.PotentialType,
            LifecycleStage = x.LifecycleStage,
            EstimatedValue = x.EstimatedValue,
            Currency = x.Currency,
            CreatedAtUtc = x.CreatedAtUtc,
            UpdatedAtUtc = x.UpdatedAtUtc
        };
    }
}
