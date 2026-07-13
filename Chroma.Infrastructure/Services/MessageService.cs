using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Messages.Dtos;
using Chroma.Application.Modules.Messages.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class MessageService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IMessageService
{
    public async Task<MessageSearchResult> SearchAsync(MessageSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Messages
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ConversationId == request.ConversationId);

        if (!string.IsNullOrWhiteSpace(request.Direction))
        {
            queryable = queryable.Where(x => x.Direction == request.Direction.Trim().ToUpperInvariant());
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderByDescending(x => x.SentAtUtc)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        return new MessageSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<MessageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.Messages
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MessageDto> SendOutboundAsync(SendOutboundMessageRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var conversation = await dbContext.Conversations
            .FirstOrDefaultAsync(x => x.Id == request.ConversationId && x.TenantId == tenantId, cancellationToken)
            ?? throw new InvalidOperationException("Conversation not found.");

        var entity = new Message
        {
            TenantId = tenantId,
            ConversationId = request.ConversationId,
            ChannelId = request.ChannelId,
            Direction = "OUT",
            MessageType = request.MessageType,
            Text = request.Text,
            MediaUrl = request.MediaUrl,
            Status = "sent",
            SentAtUtc = DateTime.UtcNow
        };

        dbContext.Messages.Add(entity);

        conversation.LastMessageAtUtc = entity.SentAtUtc;
        conversation.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    private static MessageDto ToDto(Message entity)
    {
        return new MessageDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            ConversationId = entity.ConversationId,
            ChannelId = entity.ChannelId,
            Direction = entity.Direction,
            MessageType = entity.MessageType,
            ExternalId = entity.ExternalId,
            Text = entity.Text,
            MediaUrl = entity.MediaUrl,
            Status = entity.Status,
            SentAtUtc = entity.SentAtUtc,
            DeliveredAtUtc = entity.DeliveredAtUtc,
            ReadAtUtc = entity.ReadAtUtc
        };
    }

    private static Expression<Func<Message, MessageDto>> MapToDto()
    {
        return x => new MessageDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            ConversationId = x.ConversationId,
            ChannelId = x.ChannelId,
            Direction = x.Direction,
            MessageType = x.MessageType,
            ExternalId = x.ExternalId,
            Text = x.Text,
            MediaUrl = x.MediaUrl,
            Status = x.Status,
            SentAtUtc = x.SentAtUtc,
            DeliveredAtUtc = x.DeliveredAtUtc,
            ReadAtUtc = x.ReadAtUtc
        };
    }
}
