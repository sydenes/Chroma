using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Appointments.Dtos;
using Chroma.Application.Modules.Appointments.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class AppointmentService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser) : IAppointmentService
{
    public async Task<AppointmentSearchResult> SearchAsync(AppointmentSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Appointments.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (request.FromUtc.HasValue)
        {
            queryable = queryable.Where(x => x.StartsAtUtc >= request.FromUtc.Value);
        }

        if (request.ToUtc.HasValue)
        {
            queryable = queryable.Where(x => x.StartsAtUtc <= request.ToUtc.Value);
        }

        if (request.ContactId.HasValue)
        {
            queryable = queryable.Where(x => x.ContactId == request.ContactId.Value);
        }

        if (request.OwnerId.HasValue)
        {
            queryable = queryable.Where(x => x.OwnerId == request.OwnerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            queryable = queryable.Where(x => x.Status == request.Status.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queryable = queryable.Where(x => x.Title.Contains(request.Query.Trim()));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderBy(x => x.StartsAtUtc)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        await PopulateNamesAsync(items, cancellationToken);

        return new AppointmentSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<AppointmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var appointment = await dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (appointment is not null)
        {
            await PopulateNamesAsync([appointment], cancellationToken);
        }

        return appointment;
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new Appointment
        {
            TenantId = tenantId,
            ContactId = request.ContactId,
            OwnerId = request.OwnerId ?? currentUser.UserId,
            Title = request.Title.Trim(),
            Notes = Clean(request.Notes),
            StartsAtUtc = request.StartsAtUtc,
            EndsAtUtc = request.EndsAtUtc,
            Mode = string.IsNullOrWhiteSpace(request.Mode) ? "office" : request.Mode.Trim().ToLowerInvariant()
        };

        dbContext.Appointments.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = ToDto(entity);
        await PopulateNamesAsync([dto], cancellationToken);
        return dto;
    }

    public async Task<AppointmentDto?> UpdateAsync(Guid id, UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Appointments.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var previousStatus = entity.Status;

        entity.ContactId = request.ContactId;
        entity.OwnerId = request.OwnerId ?? currentUser.UserId;
        entity.Title = request.Title.Trim();
        entity.Notes = Clean(request.Notes);
        entity.StartsAtUtc = request.StartsAtUtc;
        entity.EndsAtUtc = request.EndsAtUtc;
        entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToLowerInvariant();
        entity.Mode = string.IsNullOrWhiteSpace(request.Mode) ? entity.Mode : request.Mode.Trim().ToLowerInvariant();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        if (previousStatus != "completed" && entity.Status == "completed")
        {
            var duration = Math.Max(
                1,
                (int)Math.Round((entity.EndsAtUtc - entity.StartsAtUtc).TotalMinutes));

            dbContext.Activities.Add(new Activity
            {
                TenantId = tenantId,
                OwnerId = entity.OwnerId,
                ContactId = entity.ContactId,
                ActivityType = "session",
                Subject = entity.Title,
                Description = entity.Notes,
                OccurredAtUtc = entity.StartsAtUtc,
                DurationMinutes = duration
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = ToDto(entity);
        await PopulateNamesAsync([dto], cancellationToken);
        return dto;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Appointments.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
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
        IReadOnlyCollection<AppointmentDto> appointments,
        CancellationToken cancellationToken)
    {
        if (appointments.Count == 0)
        {
            return;
        }

        var contactIds = appointments
            .Where(x => x.ContactId.HasValue)
            .Select(x => x.ContactId!.Value)
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

            foreach (var appointment in appointments)
            {
                if (appointment.ContactId is Guid contactId && contactNameById.TryGetValue(contactId, out var name))
                {
                    appointment.ContactName = name;
                }
            }
        }

        var ownerIds = appointments
            .Where(x => x.OwnerId.HasValue)
            .Select(x => x.OwnerId!.Value)
            .Distinct()
            .ToArray();

        if (ownerIds.Length > 0)
        {
            var owners = await dbContext.Users
                .AsNoTracking()
                .Where(x => ownerIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.FirstName + " " + x.LastName })
                .ToListAsync(cancellationToken);

            var ownerNameById = owners.ToDictionary(x => x.Id, x => x.Name.Trim());

            foreach (var appointment in appointments)
            {
                if (appointment.OwnerId is Guid ownerId && ownerNameById.TryGetValue(ownerId, out var name))
                {
                    appointment.OwnerName = name;
                }
            }
        }
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AppointmentDto ToDto(Appointment entity)
    {
        return new AppointmentDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            ContactId = entity.ContactId,
            OwnerId = entity.OwnerId,
            Title = entity.Title,
            Notes = entity.Notes,
            StartsAtUtc = entity.StartsAtUtc,
            EndsAtUtc = entity.EndsAtUtc,
            Status = entity.Status,
            Mode = entity.Mode
        };
    }

    private static Expression<Func<Appointment, AppointmentDto>> MapToDto()
    {
        return x => new AppointmentDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            ContactId = x.ContactId,
            OwnerId = x.OwnerId,
            Title = x.Title,
            Notes = x.Notes,
            StartsAtUtc = x.StartsAtUtc,
            EndsAtUtc = x.EndsAtUtc,
            Status = x.Status,
            Mode = x.Mode
        };
    }
}
