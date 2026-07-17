using Chroma.Application.Abstractions;
using Chroma.Application.Common.Exceptions;
using Chroma.Application.Modules.Conversations.Dtos;
using Chroma.Application.Modules.Conversations.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class ConversationService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser) : IConversationService
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

        var kind = request.Kind?.Trim().ToLowerInvariant();
        if (kind == "team")
        {
            queryable = queryable.Where(x => x.ContactId == null);
        }
        else if (kind == "external")
        {
            queryable = queryable.Where(x => x.ContactId != null);
        }

        var mineOnly = request.MineOnly == true || kind == "team";
        if (mineOnly)
        {
            var userId = currentUser.UserId
                ?? throw new InvalidOperationException("User context is required.");

            var myConversationIds = dbContext.ConversationParticipants
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.UserId == userId)
                .Select(x => x.ConversationId);

            queryable = queryable.Where(x => myConversationIds.Contains(x.Id));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            queryable = queryable.Where(x => x.Status == request.Status.Trim().ToLowerInvariant());
        }

        if (request.HasUnread == true)
        {
            var userId = currentUser.UserId
                ?? throw new InvalidOperationException("User context is required.");

            var unreadConversationIds = dbContext.ConversationParticipants
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.UserId == userId && x.UnreadCount > 0)
                .Select(x => x.ConversationId);

            queryable = queryable.Where(x => unreadConversationIds.Contains(x.Id));
        }
        else if (request.HasUnread == false)
        {
            var userId = currentUser.UserId
                ?? throw new InvalidOperationException("User context is required.");

            var unreadConversationIds = dbContext.ConversationParticipants
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.UserId == userId && x.UnreadCount > 0)
                .Select(x => x.ConversationId);

            queryable = queryable.Where(x => !unreadConversationIds.Contains(x.Id));
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var q = request.Query.Trim();
            queryable = queryable.Where(x =>
                (x.ExternalConversationId != null && x.ExternalConversationId.Contains(q))
                || (x.Title != null && x.Title.Contains(q)));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderByDescending(x => x.LastMessageAtUtc ?? x.CreatedAtUtc)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        await PopulateDisplayNamesAsync(items, cancellationToken);

        return new ConversationSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<ConversationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var conversation = await dbContext.Conversations
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (conversation is not null)
        {
            await PopulateDisplayNamesAsync([conversation], cancellationToken);
        }

        return conversation;
    }

    public async Task<ConversationDto> CreateAsync(CreateConversationRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        if (request.PeerUserId.HasValue)
        {
            return await CreateOrGetTeamConversationAsync(tenantId, request.PeerUserId.Value, request.ChannelId, cancellationToken);
        }

        if (request.IsGroup || (request.MemberUserIds is { Count: > 0 }))
        {
            return await CreateGroupConversationAsync(tenantId, request, cancellationToken);
        }

        if (!request.ChannelId.HasValue)
        {
            throw new AppException(
                "conversations.channelRequired",
                "Channel selection is required.",
                400);
        }

        var entity = new Conversation
        {
            TenantId = tenantId,
            ChannelId = request.ChannelId.Value,
            ContactId = request.ContactId,
            AssignedUserId = request.AssignedUserId ?? currentUser.UserId,
            ExternalConversationId = request.ExternalConversationId,
            Status = "open"
        };

        dbContext.Conversations.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (currentUser.UserId.HasValue)
        {
            dbContext.ConversationParticipants.Add(new ConversationParticipant
            {
                TenantId = tenantId,
                ConversationId = entity.Id,
                UserId = currentUser.UserId,
                Role = "owner"
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var dto = ToDto(entity);
        await PopulateDisplayNamesAsync([dto], cancellationToken);
        return dto;
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
        var dto = ToDto(entity);
        await PopulateDisplayNamesAsync([dto], cancellationToken);
        return dto;
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
        var dto = ToDto(entity);
        await PopulateDisplayNamesAsync([dto], cancellationToken);
        return dto;
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
        var dto = ToDto(entity);
        await PopulateDisplayNamesAsync([dto], cancellationToken);
        return dto;
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

        var userId = currentUser.UserId;
        if (userId.HasValue)
        {
            var participant = await dbContext.ConversationParticipants
                .FirstOrDefaultAsync(
                    x => x.ConversationId == id && x.TenantId == tenantId && x.UserId == userId,
                    cancellationToken);

            if (participant is not null)
            {
                participant.UnreadCount = 0;
                participant.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        // Legacy conversation-level counter kept in sync with current viewer's unread.
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
        var dto = ToDto(entity);
        await PopulateDisplayNamesAsync([dto], cancellationToken);
        return dto;
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

    private async Task<ConversationDto> CreateGroupConversationAsync(
        Guid tenantId,
        CreateConversationRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.UserId
            ?? throw new InvalidOperationException("User context is required.");

        var title = request.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new AppException(
                "conversations.groupTitleRequired",
                "Group title is required.",
                400);
        }

        var memberIds = (request.MemberUserIds ?? [])
            .Where(id => id != Guid.Empty && id != currentUserId)
            .Distinct()
            .ToList();

        if (memberIds.Count == 0)
        {
            throw new AppException(
                "conversations.groupMembersRequired",
                "Select at least one other member for the group.",
                400);
        }

        var activeMemberCount = await dbContext.UserTenants
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId && memberIds.Contains(x.UserId) && x.Status == "active",
                cancellationToken);

        if (activeMemberCount != memberIds.Count)
        {
            throw new AppException(
                "conversations.peerNotActive",
                "One or more selected users are not active in this workspace.",
                400);
        }

        var channel = await ResolveInternalChannelAsync(tenantId, request.ChannelId, cancellationToken);

        var entity = new Conversation
        {
            TenantId = tenantId,
            ChannelId = channel.Id,
            ContactId = null,
            AssignedUserId = currentUserId,
            ExternalConversationId = $"group:{Guid.NewGuid():D}",
            Title = title,
            IsGroup = true,
            Status = "open"
        };

        dbContext.Conversations.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var participants = new List<ConversationParticipant>
        {
            new()
            {
                TenantId = tenantId,
                ConversationId = entity.Id,
                UserId = currentUserId,
                Role = "owner"
            }
        };

        participants.AddRange(memberIds.Select(memberId => new ConversationParticipant
        {
            TenantId = tenantId,
            ConversationId = entity.Id,
            UserId = memberId,
            Role = "member"
        }));

        dbContext.ConversationParticipants.AddRange(participants);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = ToDto(entity);
        await PopulateDisplayNamesAsync([dto], cancellationToken);
        return dto;
    }

    private async Task<Channel> ResolveInternalChannelAsync(
        Guid tenantId,
        Guid? channelId,
        CancellationToken cancellationToken)
    {
        var channel = channelId.HasValue
            ? await dbContext.Channels.FirstOrDefaultAsync(
                x => x.Id == channelId.Value && x.TenantId == tenantId,
                cancellationToken)
            : await dbContext.Channels.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Provider == "internal" && x.IsActive,
                cancellationToken);

        if (channel is null)
        {
            channel = new Channel
            {
                TenantId = tenantId,
                Provider = "internal",
                Name = "Ekip Sohbeti",
                IsActive = true
            };
            dbContext.Channels.Add(channel);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return channel;
    }

    private async Task<ConversationDto> CreateOrGetTeamConversationAsync(
        Guid tenantId,
        Guid peerUserId,
        Guid? channelId,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.UserId
            ?? throw new InvalidOperationException("User context is required.");

        if (peerUserId == currentUserId)
        {
            throw new AppException(
                "conversations.selfConversation",
                "You cannot start a conversation with yourself.");
        }

        var peerIsMember = await dbContext.UserTenants
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.UserId == peerUserId && x.Status == "active", cancellationToken);

        if (!peerIsMember)
        {
            throw new AppException(
                "conversations.peerNotActive",
                "The selected user is not active in this workspace.");
        }

        var channel = await ResolveInternalChannelAsync(tenantId, channelId, cancellationToken);

        var a = currentUserId;
        var b = peerUserId;
        if (a.CompareTo(b) > 0)
        {
            (a, b) = (b, a);
        }

        var externalKey = $"team:{a:D}:{b:D}";

        var existing = await dbContext.Conversations
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.ExternalConversationId == externalKey,
                cancellationToken);

        if (existing is not null)
        {
            var existingDto = ToDto(existing);
            await PopulateDisplayNamesAsync([existingDto], cancellationToken);
            return existingDto;
        }

        var entity = new Conversation
        {
            TenantId = tenantId,
            ChannelId = channel.Id,
            ContactId = null,
            AssignedUserId = peerUserId,
            ExternalConversationId = externalKey,
            IsGroup = false,
            Status = "open"
        };

        dbContext.Conversations.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.ConversationParticipants.AddRange(
            new ConversationParticipant
            {
                TenantId = tenantId,
                ConversationId = entity.Id,
                UserId = currentUserId,
                Role = "member"
            },
            new ConversationParticipant
            {
                TenantId = tenantId,
                ConversationId = entity.Id,
                UserId = peerUserId,
                Role = "member"
            });

        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = ToDto(entity);
        await PopulateDisplayNamesAsync([dto], cancellationToken);
        return dto;
    }

    private async Task PopulateDisplayNamesAsync(
        IReadOnlyCollection<ConversationDto> conversations,
        CancellationToken cancellationToken)
    {
        if (conversations.Count == 0)
        {
            return;
        }

        var contactIds = conversations
            .Where(x => x.ContactId.HasValue)
            .Select(x => x.ContactId!.Value)
            .Distinct()
            .ToArray();

        Dictionary<Guid, string> contactNames = [];
        if (contactIds.Length > 0)
        {
            contactNames = await dbContext.Contacts
                .AsNoTracking()
                .Where(x => contactIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.FirstName + " " + x.LastName).Trim(), cancellationToken);
        }

        var conversationIds = conversations.Select(x => x.Id).ToArray();
        var participants = await dbContext.ConversationParticipants
            .AsNoTracking()
            .Where(x => conversationIds.Contains(x.ConversationId) && x.UserId != null)
            .Select(x => new { x.ConversationId, UserId = x.UserId!.Value, x.UnreadCount })
            .ToListAsync(cancellationToken);

        var userIds = participants.Select(x => x.UserId).Distinct().ToArray();
        Dictionary<Guid, string> userNames = [];
        if (userIds.Length > 0)
        {
            userNames = await dbContext.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.FirstName + " " + x.LastName).Trim(), cancellationToken);
        }

        var me = currentUser.UserId;
        var myUnreadByConversation = participants
            .Where(x => me.HasValue && x.UserId == me.Value)
            .GroupBy(x => x.ConversationId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.UnreadCount));

        foreach (var conversation in conversations)
        {
            if (myUnreadByConversation.TryGetValue(conversation.Id, out var myUnread))
            {
                conversation.UnreadCount = myUnread;
            }

            if (conversation.ContactId is Guid contactId && contactNames.TryGetValue(contactId, out var contactName))
            {
                conversation.ContactName = contactName;
                conversation.Kind = "external";
                conversation.Title = contactName;
                conversation.IsGroup = false;
                conversation.ParticipantCount = 0;
                conversation.ParticipantNames = [];
                continue;
            }

            conversation.Kind = "team";
            var memberIds = participants
                .Where(x => x.ConversationId == conversation.Id)
                .Select(x => x.UserId)
                .Distinct()
                .ToArray();

            conversation.ParticipantCount = memberIds.Length;
            conversation.ParticipantNames = memberIds
                .Where(id => userNames.ContainsKey(id))
                .Select(id => userNames[id])
                .OrderBy(name => name)
                .ToArray();

            if (conversation.IsGroup)
            {
                if (string.IsNullOrWhiteSpace(conversation.Title))
                {
                    conversation.Title = string.Join(", ", conversation.ParticipantNames.Take(3));
                    if (conversation.ParticipantNames.Count > 3)
                    {
                        conversation.Title += $" +{conversation.ParticipantNames.Count - 3}";
                    }
                }

                continue;
            }

            var peerId = memberIds
                .Where(id => id != me)
                .Select(id => (Guid?)id)
                .FirstOrDefault()
                ?? conversation.AssignedUserId;

            if (peerId is Guid peer && userNames.TryGetValue(peer, out var peerName))
            {
                conversation.PeerUserId = peer;
                conversation.PeerUserName = peerName;
                conversation.Title = peerName;
            }
            else
            {
                conversation.Title = "Ekip sohbeti";
            }
        }
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
            LastMessageAtUtc = entity.LastMessageAtUtc,
            Title = entity.Title ?? string.Empty,
            IsGroup = entity.IsGroup,
            Kind = entity.ContactId is null ? "team" : "external"
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
            LastMessageAtUtc = x.LastMessageAtUtc,
            Title = x.Title ?? string.Empty,
            IsGroup = x.IsGroup,
            Kind = x.ContactId == null ? "team" : "external"
        };
    }
}
