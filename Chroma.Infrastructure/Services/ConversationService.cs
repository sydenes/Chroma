using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Conversations.Dtos;
using Chroma.Application.Modules.Conversations.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class ConversationService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IConversationService
{
    public async Task<ConversationSearchResult> SearchAsync(ConversationSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Conversations.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (request.ChannelId.HasValue)
        {
            queryable = queryable.Where(x => x.ChannelId == request.ChannelId.Value);
        }

        if (request.ContactId.HasValue)
        {
            queryable = queryable.Where(x => x.ContactId == request.ContactId.Value);
        }

        if (request.AssignedUserId.HasValue)
        {
            queryable = queryable.Where(x => x.AssignedUserId == request.AssignedUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            queryable = queryable.Where(x => x.Status == request.Status.Trim().ToLowerInvariant());
        }

        if (request.HasUnread == true)
        {
            queryable = queryable.Where(x => x.UnreadCount > 0);
        }
        else if (request.HasUnread == false)
        {
            queryable = queryable.Where(x => x.UnreadCount == 0);
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queryable = queryable.Where(x => x.ExternalConversationId != null && x.ExternalConversationId.Contains(request.Query.Trim()));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderByDescending(x => x.LastMessageAtUtc ?? x.CreatedAtUtc)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        return new ConversationSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<ConversationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.Conversations
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ConversationDto> CreateAsync(CreateConversationRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new Conversation
        {
            TenantId = tenantId,
            ChannelId = request.ChannelId,
            ContactId = request.ContactId,
            AssignedUserId = request.AssignedUserId,
            ExternalConversationId = request.ExternalConversationId,
            Status = "open"
        };

        dbContext.Conversations.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<ConversationDto?> UpdateAsync(Guid id, UpdateConversationRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.ContactId = request.ContactId;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<ConversationDto?> AssignAsync(Guid id, AssignConversationRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.AssignedUserId = request.AssignedUserId;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<ConversationDto?> UpdateStatusAsync(Guid id, UpdateConversationStatusRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Status = request.Status.Trim().ToLowerInvariant();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<ConversationDto?> MarkAsReadAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.UnreadCount = 0;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        var messages = await dbContext.Messages
            .Where(x => x.ConversationId == id && x.TenantId == tenantId && x.Direction == "IN" && x.ReadAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.ReadAtUtc = DateTime.UtcNow;
            message.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
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

    private static ConversationDto ToDto(Conversation entity)
    {
        return new ConversationDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            ChannelId = entity.ChannelId,
            ContactId = entity.ContactId,
            AssignedUserId = entity.AssignedUserId,
            Status = entity.Status,
            UnreadCount = entity.UnreadCount,
            ExternalConversationId = entity.ExternalConversationId,
            LastMessageAtUtc = entity.LastMessageAtUtc
        };
    }

    private static Expression<Func<Conversation, ConversationDto>> MapToDto()
    {
        return x => new ConversationDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            ChannelId = x.ChannelId,
            ContactId = x.ContactId,
            AssignedUserId = x.AssignedUserId,
            Status = x.Status,
            UnreadCount = x.UnreadCount,
            ExternalConversationId = x.ExternalConversationId,
            LastMessageAtUtc = x.LastMessageAtUtc
        };
    }
}
