using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Notes.Dtos;
using Chroma.Application.Modules.Notes.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class NoteService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : INoteService
{
    public async Task<NoteSearchResult> SearchAsync(NoteSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Notes.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.OwnerType))
        {
            queryable = queryable.Where(x => x.OwnerType == request.OwnerType.Trim());
        }

        if (request.OwnerId.HasValue)
        {
            queryable = queryable.Where(x => x.OwnerId == request.OwnerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queryable = queryable.Where(x => x.Content.Contains(request.Query.Trim()));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        await PopulateNamesAsync(items, cancellationToken);

        return new NoteSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<NoteDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var note = await dbContext.Notes
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (note is not null)
        {
            await PopulateNamesAsync([note], cancellationToken);
        }

        return note;
    }

    public async Task<NoteDto> CreateAsync(CreateNoteRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new Note
        {
            TenantId = tenantId,
            AuthorId = request.AuthorId,
            OwnerType = request.OwnerType.Trim(),
            OwnerId = request.OwnerId,
            Content = request.Content.Trim()
        };

        dbContext.Notes.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = ToDto(entity);
        await PopulateNamesAsync([dto], cancellationToken);
        return dto;
    }

    public async Task<NoteDto?> UpdateAsync(Guid id, UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Notes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Content = request.Content.Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = ToDto(entity);
        await PopulateNamesAsync([dto], cancellationToken);
        return dto;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Notes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
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

    private async Task PopulateNamesAsync(
        IReadOnlyCollection<NoteDto> notes,
        CancellationToken cancellationToken)
    {
        if (notes.Count == 0)
        {
            return;
        }

        var authorIds = notes
            .Where(x => x.AuthorId.HasValue)
            .Select(x => x.AuthorId!.Value)
            .Distinct()
            .ToArray();

        if (authorIds.Length > 0)
        {
            var authors = await dbContext.Users
                .AsNoTracking()
                .Where(x => authorIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.FirstName + " " + x.LastName })
                .ToListAsync(cancellationToken);

            var authorNameById = authors.ToDictionary(x => x.Id, x => x.Name.Trim());

            foreach (var note in notes)
            {
                if (note.AuthorId is Guid authorId && authorNameById.TryGetValue(authorId, out var name))
                {
                    note.AuthorName = name;
                }
            }
        }

        var contactIds = notes
            .Where(x => x.OwnerType == "contact")
            .Select(x => x.OwnerId)
            .Distinct()
            .ToArray();

        if (contactIds.Length > 0)
        {
            var contacts = await dbContext.Contacts
                .AsNoTracking()
                .Where(x => contactIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.FirstName + " " + x.LastName })
                .ToListAsync(cancellationToken);

            var contactNameById = contacts.ToDictionary(x => x.Id, x => x.Name.Trim());

            foreach (var note in notes.Where(x => x.OwnerType == "contact"))
            {
                if (contactNameById.TryGetValue(note.OwnerId, out var name))
                {
                    note.ContactName = name;
                }
            }
        }
    }

    private static NoteDto ToDto(Note entity)
    {
        return new NoteDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            AuthorId = entity.AuthorId,
            OwnerType = entity.OwnerType,
            OwnerId = entity.OwnerId,
            Content = entity.Content,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    private static Expression<Func<Note, NoteDto>> MapToDto()
    {
        return x => new NoteDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            AuthorId = x.AuthorId,
            OwnerType = x.OwnerType,
            OwnerId = x.OwnerId,
            Content = x.Content,
            CreatedAtUtc = x.CreatedAtUtc,
            UpdatedAtUtc = x.UpdatedAtUtc
        };
    }
}
