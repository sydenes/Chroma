using Chroma.Application.Abstractions;
using Chroma.Application.Common.Exceptions;
using Chroma.Application.Modules.Messages.Dtos;
using Chroma.Application.Modules.Messages.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class MessageService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser) : IMessageService
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
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        await PopulateFileMetadataAsync(items, cancellationToken);

        return new MessageSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<MessageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var message = await dbContext.Messages
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (message is not null)
        {
            await PopulateFileMetadataAsync([message], cancellationToken);
        }

        return message;
    }

    public async Task<MessageDto> SendOutboundAsync(SendOutboundMessageRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var senderUserId = currentUser.UserId
            ?? throw new InvalidOperationException("Authenticated user is required to send messages.");

        var conversation = await dbContext.Conversations
            .FirstOrDefaultAsync(x => x.Id == request.ConversationId && x.TenantId == tenantId, cancellationToken)
            ?? throw new InvalidOperationException("Conversation not found.");

        var sender = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == senderUserId)
            .Select(x => new { x.FirstName, x.LastName })
            .FirstOrDefaultAsync(cancellationToken);

        var displayName = sender is null
            ? null
            : $"{sender.FirstName} {sender.LastName}".Trim();

        string? text = string.IsNullOrWhiteSpace(request.Text) ? null : request.Text.Trim();
        string? mediaUrl = string.IsNullOrWhiteSpace(request.MediaUrl) ? null : request.MediaUrl.Trim();
        Guid? fileId = request.FileId;
        var messageType = string.IsNullOrWhiteSpace(request.MessageType)
            ? "text"
            : request.MessageType.Trim().ToLowerInvariant();

        StoredFile? file = null;
        if (fileId.HasValue)
        {
            file = await dbContext.StoredFiles
                .FirstOrDefaultAsync(x => x.Id == fileId.Value && x.TenantId == tenantId, cancellationToken)
                ?? throw new AppException("messages.fileNotFound", "Attached file was not found.", 404);

            mediaUrl ??= $"/api/files/{file.Id}/download";
            text ??= file.FileName;
            if (messageType is "text" or "")
            {
                messageType = file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                    ? "image"
                    : "file";
            }

            // Bind attachment to this conversation for ownership consistency.
            if (!string.Equals(file.OwnerType, "conversation", StringComparison.OrdinalIgnoreCase)
                || file.OwnerId != conversation.Id)
            {
                file.OwnerType = "conversation";
                file.OwnerId = conversation.Id;
                file.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(mediaUrl) && !fileId.HasValue)
        {
            throw new AppException(
                "messages.textOrMediaUrlRequired",
                "Text or media URL is required.",
                400);
        }

        var entity = new Message
        {
            TenantId = tenantId,
            ConversationId = request.ConversationId,
            ChannelId = request.ChannelId,
            Direction = "OUT",
            SenderUserId = senderUserId,
            SenderDisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName,
            MessageType = messageType,
            Text = text,
            MediaUrl = mediaUrl,
            FileId = fileId,
            Status = "sent",
            SentAtUtc = DateTime.UtcNow
        };

        dbContext.Messages.Add(entity);

        conversation.LastMessageAtUtc = entity.SentAtUtc;
        conversation.LastMessagePreview = CreatePreview(text, file?.FileName);
        conversation.UpdatedAtUtc = DateTime.UtcNow;

        var recipients = await dbContext.ConversationParticipants
            .Where(x =>
                x.ConversationId == conversation.Id
                && x.TenantId == tenantId
                && x.UserId != null
                && x.UserId != senderUserId)
            .ToListAsync(cancellationToken);

        foreach (var recipient in recipients)
        {
            recipient.UnreadCount += 1;
            recipient.UpdatedAtUtc = DateTime.UtcNow;
        }

        // Legacy aggregate for non-participant UIs.
        conversation.UnreadCount = recipients.Sum(x => x.UnreadCount);

        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = ToDto(entity);
        if (file is not null)
        {
            dto.FileName = file.FileName;
            dto.ContentType = file.ContentType;
        }

        return dto;
    }

    private static string CreatePreview(string? text, string? fileName)
    {
        var preview = !string.IsNullOrWhiteSpace(text)
            ? text.Trim()
            : fileName ?? string.Empty;

        preview = string.Join(' ', preview.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return preview.Length <= 500 ? preview : preview[..497] + "...";
    }

    private async Task PopulateFileMetadataAsync(
        IReadOnlyCollection<MessageDto> messages,
        CancellationToken cancellationToken)
    {
        var fileIds = messages
            .Where(x => x.FileId.HasValue)
            .Select(x => x.FileId!.Value)
            .Distinct()
            .ToArray();

        if (fileIds.Length == 0)
        {
            return;
        }

        var files = await dbContext.StoredFiles
            .AsNoTracking()
            .Where(x => fileIds.Contains(x.Id))
            .Select(x => new { x.Id, x.FileName, x.ContentType })
            .ToListAsync(cancellationToken);

        var byId = files.ToDictionary(x => x.Id);

        foreach (var message in messages)
        {
            if (message.FileId is Guid id && byId.TryGetValue(id, out var file))
            {
                message.FileName = file.FileName;
                message.ContentType = file.ContentType;
            }
        }
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
            SenderUserId = entity.SenderUserId,
            SenderDisplayName = entity.SenderDisplayName,
            MessageType = entity.MessageType,
            ExternalId = entity.ExternalId,
            Text = entity.Text,
            MediaUrl = entity.MediaUrl,
            FileId = entity.FileId,
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
            SenderUserId = x.SenderUserId,
            SenderDisplayName = x.SenderDisplayName,
            MessageType = x.MessageType,
            ExternalId = x.ExternalId,
            Text = x.Text,
            MediaUrl = x.MediaUrl,
            FileId = x.FileId,
            Status = x.Status,
            SentAtUtc = x.SentAtUtc,
            DeliveredAtUtc = x.DeliveredAtUtc,
            ReadAtUtc = x.ReadAtUtc
        };
    }
}
