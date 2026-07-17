using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Notifications.Dtos;
using Chroma.Application.Modules.Notifications.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class NotificationService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser) : INotificationService
{
    public async Task<NotificationSearchResult> SearchAsync(NotificationSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var userId = request.UserId ?? currentUser.UserId
            ?? throw new InvalidOperationException("User context is required.");

        var queryable = dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId);

        if (request.IsRead.HasValue)
        {
            queryable = queryable.Where(x => x.IsRead == request.IsRead.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.NotificationType))
        {
            queryable = queryable.Where(x => x.NotificationType == request.NotificationType.Trim());
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        return new NotificationSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<NotificationUnreadCountResult> GetUnreadCountAsync(CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("User context is required.");

        var unreadCount = await dbContext.Notifications
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && x.UserId == userId && !x.IsRead, cancellationToken);

        return new NotificationUnreadCountResult { UnreadCount = unreadCount };
    }

    public async Task<NotificationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new Notification
        {
            TenantId = tenantId,
            UserId = request.UserId,
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            NotificationType = request.NotificationType,
            SourceType = string.IsNullOrWhiteSpace(request.SourceType) ? null : request.SourceType.Trim(),
            SourceId = request.SourceId
        };

        dbContext.Notifications.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<NotificationDto?> MarkAsReadAsync(Guid id, MarkNotificationReadRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("User context is required.");

        var entity = await dbContext.Notifications.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && x.UserId == userId,
            cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.IsRead = request.IsRead;
        entity.ReadAtUtc = request.IsRead ? DateTime.UtcNow : null;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<int> MarkAllAsReadAsync(CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("User context is required.");

        var unread = await dbContext.Notifications
            .Where(x => x.TenantId == tenantId && x.UserId == userId && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = now;
            notification.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return unread.Count;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("User context is required.");

        var entity = await dbContext.Notifications.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && x.UserId == userId,
            cancellationToken);

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

    private static NotificationDto ToDto(Notification entity)
    {
        return new NotificationDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            UserId = entity.UserId,
            Title = entity.Title,
            Body = entity.Body,
            NotificationType = entity.NotificationType,
            SourceType = entity.SourceType,
            SourceId = entity.SourceId,
            IsRead = entity.IsRead,
            ReadAtUtc = entity.ReadAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    private static Expression<Func<Notification, NotificationDto>> MapToDto()
    {
        return x => new NotificationDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            UserId = x.UserId,
            Title = x.Title,
            Body = x.Body,
            NotificationType = x.NotificationType,
            SourceType = x.SourceType,
            SourceId = x.SourceId,
            IsRead = x.IsRead,
            ReadAtUtc = x.ReadAtUtc,
            CreatedAtUtc = x.CreatedAtUtc
        };
    }
}
