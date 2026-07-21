using Chroma.Application.Abstractions;
using Chroma.Application.Common.Exceptions;
using Chroma.Application.Modules.TaskBoards.Dtos;
using Chroma.Application.Modules.TaskBoards.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Infrastructure.Services;

public class TaskBoardService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser) : ITaskBoardService
{
    private static readonly (string Name, string? Color)[] DefaultColumns =
    [
        ("Yapılacak", "#94a3b8"),
        ("Devam Ediyor", "#3b82f6"),
        ("Test", "#f59e0b"),
        ("Tamamlandı", "#22c55e")
    ];

    private static readonly (string Name, string Color)[] DefaultLabels =
    [
        ("Acil", "#ef4444"),
        ("İyileştirme", "#8b5cf6"),
        ("Hata", "#f97316"),
        ("Dokümantasyon", "#06b6d4")
    ];

    public async Task<TaskBoardDto> GetDefaultBoardAsync(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var board = await EnsureDefaultBoardAsync(tenantId, cancellationToken);
        return await MapBoardAsync(board, cancellationToken);
    }

    public async Task<TaskColumnDto> CreateColumnAsync(
        Guid boardId,
        CreateTaskColumnRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var board = await RequireBoardAsync(boardId, tenantId, cancellationToken);
        var name = RequireTitle(request.Name, "taskboards.columnNameRequired", "Column name is required.");

        var maxOrder = await dbContext.TaskColumns
            .Where(x => x.BoardId == board.Id && x.TenantId == tenantId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken) ?? -1;

        var column = new TaskColumn
        {
            TenantId = tenantId,
            BoardId = board.Id,
            Name = name,
            Color = Clean(request.Color),
            SortOrder = maxOrder + 1
        };

        dbContext.TaskColumns.Add(column);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TaskColumnDto
        {
            Id = column.Id,
            BoardId = column.BoardId,
            Name = column.Name,
            SortOrder = column.SortOrder,
            Color = column.Color,
            Cards = []
        };
    }

    public async Task<TaskColumnDto?> UpdateColumnAsync(
        Guid columnId,
        UpdateTaskColumnRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var column = await dbContext.TaskColumns
            .FirstOrDefaultAsync(x => x.Id == columnId && x.TenantId == tenantId, cancellationToken);
        if (column is null) return null;

        column.Name = RequireTitle(request.Name, "taskboards.columnNameRequired", "Column name is required.");
        column.Color = Clean(request.Color);
        if (request.SortOrder.HasValue) column.SortOrder = request.SortOrder.Value;
        column.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new TaskColumnDto
        {
            Id = column.Id,
            BoardId = column.BoardId,
            Name = column.Name,
            SortOrder = column.SortOrder,
            Color = column.Color,
            Cards = []
        };
    }

    public async Task<bool> DeleteColumnAsync(Guid columnId, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var column = await dbContext.TaskColumns
            .FirstOrDefaultAsync(x => x.Id == columnId && x.TenantId == tenantId, cancellationToken);
        if (column is null) return false;

        var hasCards = await dbContext.TaskCards
            .AnyAsync(x => x.ColumnId == columnId && x.TenantId == tenantId, cancellationToken);
        if (hasCards)
        {
            throw new AppException(
                "taskboards.columnNotEmpty",
                "Move or delete cards before removing this column.",
                400);
        }

        column.IsDeleted = true;
        column.DeletedAtUtc = DateTime.UtcNow;
        column.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TaskCardDetailDto> CreateCardAsync(
        CreateTaskCardRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var userId = RequireUser();
        var title = RequireTitle(request.Title, "taskboards.cardTitleRequired", "Card title is required.");

        var column = await dbContext.TaskColumns
            .FirstOrDefaultAsync(x => x.Id == request.ColumnId && x.TenantId == tenantId, cancellationToken)
            ?? throw new AppException("taskboards.columnNotFound", "Column not found.", 404);

        await ValidateAssigneeAsync(tenantId, request.AssigneeUserId, cancellationToken);
        await ValidateContactAsync(tenantId, request.ContactId, cancellationToken);

        var maxOrder = await dbContext.TaskCards
            .Where(x => x.ColumnId == column.Id && x.TenantId == tenantId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken) ?? -1;

        var card = new TaskCard
        {
            TenantId = tenantId,
            BoardId = column.BoardId,
            ColumnId = column.Id,
            Title = title,
            Description = Clean(request.Description),
            SortOrder = maxOrder + 1,
            CreatedByUserId = userId,
            AssigneeUserId = request.AssigneeUserId,
            ContactId = request.ContactId,
            Priority = NormalizePriority(request.Priority)
        };

        dbContext.TaskCards.Add(card);
        await dbContext.SaveChangesAsync(cancellationToken);
        await ReplaceLabelsAsync(card, request.LabelIds, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetCardByIdAsync(card.Id, cancellationToken))!;
    }

    public async Task<TaskCardDetailDto?> GetCardByIdAsync(Guid cardId, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var card = await dbContext.TaskCards
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == cardId && x.TenantId == tenantId, cancellationToken);
        if (card is null) return null;

        var dto = await MapCardDetailAsync(card, cancellationToken);
        return dto;
    }

    public async Task<TaskCardDetailDto?> UpdateCardAsync(
        Guid cardId,
        UpdateTaskCardRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var card = await dbContext.TaskCards
            .FirstOrDefaultAsync(x => x.Id == cardId && x.TenantId == tenantId, cancellationToken);
        if (card is null) return null;

        var column = await dbContext.TaskColumns
            .FirstOrDefaultAsync(x => x.Id == request.ColumnId && x.TenantId == tenantId, cancellationToken)
            ?? throw new AppException("taskboards.columnNotFound", "Column not found.", 404);

        if (column.BoardId != card.BoardId)
        {
            throw new AppException("taskboards.columnWrongBoard", "Column does not belong to this board.", 400);
        }

        await ValidateAssigneeAsync(tenantId, request.AssigneeUserId, cancellationToken);
        await ValidateContactAsync(tenantId, request.ContactId, cancellationToken);

        card.ColumnId = column.Id;
        card.Title = RequireTitle(request.Title, "taskboards.cardTitleRequired", "Card title is required.");
        card.Description = Clean(request.Description);
        card.AssigneeUserId = request.AssigneeUserId;
        card.ContactId = request.ContactId;
        card.Priority = NormalizePriority(request.Priority);
        card.DueAtUtc = request.DueAtUtc;
        card.UpdatedAtUtc = DateTime.UtcNow;

        var doneColumn = await dbContext.TaskColumns
            .AsNoTracking()
            .Where(x => x.BoardId == card.BoardId && x.TenantId == tenantId)
            .OrderByDescending(x => x.SortOrder)
            .Select(x => new { x.Id, x.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (doneColumn is not null
            && column.Id == doneColumn.Id
            && column.Name.Contains("Tamam", StringComparison.OrdinalIgnoreCase))
        {
            card.CompletedAtUtc ??= DateTime.UtcNow;
        }
        else if (card.CompletedAtUtc is not null && column.Id != doneColumn?.Id)
        {
            card.CompletedAtUtc = null;
        }

        await ReplaceLabelsAsync(card, request.LabelIds, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetCardByIdAsync(card.Id, cancellationToken);
    }

    public async Task<TaskCardSummaryDto?> MoveCardAsync(
        Guid cardId,
        MoveTaskCardRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var card = await dbContext.TaskCards
            .FirstOrDefaultAsync(x => x.Id == cardId && x.TenantId == tenantId, cancellationToken);
        if (card is null) return null;

        var targetColumn = await dbContext.TaskColumns
            .FirstOrDefaultAsync(x => x.Id == request.ColumnId && x.TenantId == tenantId, cancellationToken)
            ?? throw new AppException("taskboards.columnNotFound", "Column not found.", 404);

        if (targetColumn.BoardId != card.BoardId)
        {
            throw new AppException("taskboards.columnWrongBoard", "Column does not belong to this board.", 400);
        }

        var siblings = await dbContext.TaskCards
            .Where(x => x.ColumnId == targetColumn.Id && x.TenantId == tenantId && x.Id != card.Id)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        var insertAt = Math.Clamp(request.SortOrder, 0, siblings.Count);
        siblings.Insert(insertAt, card);

        card.ColumnId = targetColumn.Id;
        card.UpdatedAtUtc = DateTime.UtcNow;

        if (targetColumn.Name.Contains("Tamam", StringComparison.OrdinalIgnoreCase))
        {
            card.CompletedAtUtc ??= DateTime.UtcNow;
        }
        else
        {
            card.CompletedAtUtc = null;
        }

        for (var i = 0; i < siblings.Count; i++)
        {
            siblings[i].SortOrder = i;
            siblings[i].UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var board = await MapBoardAsync(
            await RequireBoardAsync(card.BoardId, tenantId, cancellationToken),
            cancellationToken);

        return board.Columns
            .SelectMany(c => c.Cards)
            .FirstOrDefault(c => c.Id == card.Id);
    }

    public async Task<bool> DeleteCardAsync(Guid cardId, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var card = await dbContext.TaskCards
            .FirstOrDefaultAsync(x => x.Id == cardId && x.TenantId == tenantId, cancellationToken);
        if (card is null) return false;

        card.IsDeleted = true;
        card.DeletedAtUtc = DateTime.UtcNow;
        card.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TaskLabelDto> CreateLabelAsync(
        Guid boardId,
        CreateTaskLabelRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        await RequireBoardAsync(boardId, tenantId, cancellationToken);
        var name = RequireTitle(request.Name, "taskboards.labelNameRequired", "Label name is required.");

        var label = new TaskLabel
        {
            TenantId = tenantId,
            BoardId = boardId,
            Name = name,
            Color = string.IsNullOrWhiteSpace(request.Color) ? "#64748b" : request.Color.Trim()
        };

        dbContext.TaskLabels.Add(label);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TaskLabelDto
        {
            Id = label.Id,
            BoardId = label.BoardId,
            Name = label.Name,
            Color = label.Color
        };
    }

    public async Task<TaskLabelDto?> UpdateLabelAsync(
        Guid labelId,
        UpdateTaskLabelRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var label = await dbContext.TaskLabels
            .FirstOrDefaultAsync(x => x.Id == labelId && x.TenantId == tenantId, cancellationToken);
        if (label is null) return null;

        label.Name = RequireTitle(request.Name, "taskboards.labelNameRequired", "Label name is required.");
        label.Color = string.IsNullOrWhiteSpace(request.Color) ? "#64748b" : request.Color.Trim();
        label.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TaskLabelDto
        {
            Id = label.Id,
            BoardId = label.BoardId,
            Name = label.Name,
            Color = label.Color
        };
    }

    public async Task<bool> DeleteLabelAsync(Guid labelId, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var label = await dbContext.TaskLabels
            .FirstOrDefaultAsync(x => x.Id == labelId && x.TenantId == tenantId, cancellationToken);
        if (label is null) return false;

        var links = await dbContext.TaskCardLabels.Where(x => x.LabelId == labelId).ToListAsync(cancellationToken);
        dbContext.TaskCardLabels.RemoveRange(links);

        label.IsDeleted = true;
        label.DeletedAtUtc = DateTime.UtcNow;
        label.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TaskCommentDto> AddCommentAsync(
        Guid cardId,
        CreateTaskCommentRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var userId = RequireUser();
        var body = RequireTitle(request.Body, "taskboards.commentRequired", "Comment is required.");

        var card = await dbContext.TaskCards
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == cardId && x.TenantId == tenantId, cancellationToken)
            ?? throw new AppException("taskboards.cardNotFound", "Card not found.", 404);

        var comment = new TaskComment
        {
            TenantId = tenantId,
            CardId = card.Id,
            AuthorUserId = userId,
            Body = body
        };

        dbContext.TaskComments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);

        var author = await dbContext.Users.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => (x.FirstName + " " + x.LastName).Trim())
            .FirstOrDefaultAsync(cancellationToken);

        return new TaskCommentDto
        {
            Id = comment.Id,
            CardId = comment.CardId,
            AuthorUserId = comment.AuthorUserId,
            AuthorName = author,
            Body = comment.Body,
            CreatedAtUtc = comment.CreatedAtUtc
        };
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var userId = RequireUser();
        var comment = await dbContext.TaskComments
            .FirstOrDefaultAsync(x => x.Id == commentId && x.TenantId == tenantId, cancellationToken);
        if (comment is null) return false;

        if (comment.AuthorUserId != userId)
        {
            throw new AppException("taskboards.commentForbidden", "You can only delete your own comments.", 403);
        }

        comment.IsDeleted = true;
        comment.DeletedAtUtc = DateTime.UtcNow;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<TaskBoard> EnsureDefaultBoardAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var board = await dbContext.TaskBoards
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IsDefault, cancellationToken);

        if (board is not null)
        {
            var hasColumns = await dbContext.TaskColumns
                .AnyAsync(x => x.BoardId == board.Id && x.TenantId == tenantId, cancellationToken);
            if (!hasColumns)
            {
                await SeedColumnsAndLabelsAsync(board, cancellationToken);
            }

            return board;
        }

        board = new TaskBoard
        {
            TenantId = tenantId,
            Title = "Görevler",
            IsDefault = true
        };
        dbContext.TaskBoards.Add(board);
        await dbContext.SaveChangesAsync(cancellationToken);
        await SeedColumnsAndLabelsAsync(board, cancellationToken);
        return board;
    }

    private async Task SeedColumnsAndLabelsAsync(TaskBoard board, CancellationToken cancellationToken)
    {
        for (var i = 0; i < DefaultColumns.Length; i++)
        {
            var (name, color) = DefaultColumns[i];
            dbContext.TaskColumns.Add(new TaskColumn
            {
                TenantId = board.TenantId,
                BoardId = board.Id,
                Name = name,
                Color = color,
                SortOrder = i
            });
        }

        var hasLabels = await dbContext.TaskLabels
            .AnyAsync(x => x.BoardId == board.Id && x.TenantId == board.TenantId, cancellationToken);
        if (!hasLabels)
        {
            foreach (var (name, color) in DefaultLabels)
            {
                dbContext.TaskLabels.Add(new TaskLabel
                {
                    TenantId = board.TenantId,
                    BoardId = board.Id,
                    Name = name,
                    Color = color
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<TaskBoardDto> MapBoardAsync(TaskBoard board, CancellationToken cancellationToken)
    {
        var tenantId = board.TenantId;

        var columns = await dbContext.TaskColumns
            .AsNoTracking()
            .Where(x => x.BoardId == board.Id && x.TenantId == tenantId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new TaskColumnDto
            {
                Id = x.Id,
                BoardId = x.BoardId,
                Name = x.Name,
                SortOrder = x.SortOrder,
                Color = x.Color
            })
            .ToListAsync(cancellationToken);

        var labels = await dbContext.TaskLabels
            .AsNoTracking()
            .Where(x => x.BoardId == board.Id && x.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .Select(x => new TaskLabelDto
            {
                Id = x.Id,
                BoardId = x.BoardId,
                Name = x.Name,
                Color = x.Color
            })
            .ToListAsync(cancellationToken);

        var cards = await dbContext.TaskCards
            .AsNoTracking()
            .Where(x => x.BoardId == board.Id && x.TenantId == tenantId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        var cardIds = cards.Select(x => x.Id).ToArray();
        var labelLinks = await dbContext.TaskCardLabels
            .AsNoTracking()
            .Where(x => cardIds.Contains(x.CardId))
            .ToListAsync(cancellationToken);

        var commentCounts = await dbContext.TaskComments
            .AsNoTracking()
            .Where(x => cardIds.Contains(x.CardId) && x.TenantId == tenantId)
            .GroupBy(x => x.CardId)
            .Select(g => new { CardId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CardId, x => x.Count, cancellationToken);

        var attachmentCounts = await dbContext.StoredFiles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.OwnerType == "task_card" && cardIds.Contains(x.OwnerId))
            .GroupBy(x => x.OwnerId)
            .Select(g => new { OwnerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OwnerId, x => x.Count, cancellationToken);

        var userIds = cards
            .SelectMany(c => new Guid?[] { c.AssigneeUserId, c.CreatedByUserId })
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();

        var userNames = userIds.Length == 0
            ? new Dictionary<Guid, string>()
            : await dbContext.Users.AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.FirstName + " " + x.LastName).Trim(), cancellationToken);

        var contactIds = cards.Where(c => c.ContactId.HasValue).Select(c => c.ContactId!.Value).Distinct().ToArray();
        var contactNames = contactIds.Length == 0
            ? new Dictionary<Guid, string>()
            : await dbContext.Contacts.AsNoTracking()
                .Where(x => contactIds.Contains(x.Id) && x.TenantId == tenantId)
                .ToDictionaryAsync(x => x.Id, x => (x.FirstName + " " + x.LastName).Trim(), cancellationToken);

        var labelsById = labels.ToDictionary(x => x.Id);
        var labelsByCard = labelLinks
            .GroupBy(x => x.CardId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<TaskLabelDto>)g
                    .Where(l => labelsById.ContainsKey(l.LabelId))
                    .Select(l => labelsById[l.LabelId])
                    .ToArray());

        foreach (var column in columns)
        {
            column.Cards = cards
                .Where(c => c.ColumnId == column.Id)
                .OrderBy(c => c.SortOrder)
                .Select(c => new TaskCardSummaryDto
                {
                    Id = c.Id,
                    BoardId = c.BoardId,
                    ColumnId = c.ColumnId,
                    Title = c.Title,
                    SortOrder = c.SortOrder,
                    AssigneeUserId = c.AssigneeUserId,
                    AssigneeName = c.AssigneeUserId is Guid a && userNames.TryGetValue(a, out var an) ? an : null,
                    ContactId = c.ContactId,
                    ContactName = c.ContactId is Guid cid && contactNames.TryGetValue(cid, out var cn) ? cn : null,
                    Priority = c.Priority,
                    DueAtUtc = c.DueAtUtc,
                    CompletedAtUtc = c.CompletedAtUtc,
                    Labels = labelsByCard.GetValueOrDefault(c.Id, []),
                    CommentCount = commentCounts.GetValueOrDefault(c.Id),
                    AttachmentCount = attachmentCounts.GetValueOrDefault(c.Id)
                })
                .ToArray();
        }

        return new TaskBoardDto
        {
            Id = board.Id,
            TenantId = board.TenantId,
            Title = board.Title,
            IsDefault = board.IsDefault,
            Columns = columns,
            Labels = labels
        };
    }

    private async Task<TaskCardDetailDto> MapCardDetailAsync(TaskCard card, CancellationToken cancellationToken)
    {
        var labelIds = await dbContext.TaskCardLabels
            .AsNoTracking()
            .Where(x => x.CardId == card.Id)
            .Select(x => x.LabelId)
            .ToListAsync(cancellationToken);

        var labels = await dbContext.TaskLabels
            .AsNoTracking()
            .Where(x => labelIds.Contains(x.Id) && x.TenantId == card.TenantId)
            .Select(x => new TaskLabelDto
            {
                Id = x.Id,
                BoardId = x.BoardId,
                Name = x.Name,
                Color = x.Color
            })
            .ToListAsync(cancellationToken);

        var comments = await dbContext.TaskComments
            .AsNoTracking()
            .Where(x => x.CardId == card.Id && x.TenantId == card.TenantId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var authorIds = comments.Select(c => c.AuthorUserId).Append(card.CreatedByUserId)
            .Concat(card.AssigneeUserId.HasValue ? [card.AssigneeUserId.Value] : Array.Empty<Guid>())
            .Distinct()
            .ToArray();

        var userNames = await dbContext.Users.AsNoTracking()
            .Where(x => authorIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => (x.FirstName + " " + x.LastName).Trim(), cancellationToken);

        string? contactName = null;
        if (card.ContactId is Guid contactId)
        {
            contactName = await dbContext.Contacts.AsNoTracking()
                .Where(x => x.Id == contactId && x.TenantId == card.TenantId)
                .Select(x => (x.FirstName + " " + x.LastName).Trim())
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new TaskCardDetailDto
        {
            Id = card.Id,
            TenantId = card.TenantId,
            BoardId = card.BoardId,
            ColumnId = card.ColumnId,
            Title = card.Title,
            Description = card.Description,
            SortOrder = card.SortOrder,
            CreatedByUserId = card.CreatedByUserId,
            CreatedByName = userNames.GetValueOrDefault(card.CreatedByUserId),
            AssigneeUserId = card.AssigneeUserId,
            AssigneeName = card.AssigneeUserId is Guid a ? userNames.GetValueOrDefault(a) : null,
            ContactId = card.ContactId,
            ContactName = contactName,
            Priority = card.Priority,
            DueAtUtc = card.DueAtUtc,
            CompletedAtUtc = card.CompletedAtUtc,
            CreatedAtUtc = card.CreatedAtUtc,
            UpdatedAtUtc = card.UpdatedAtUtc,
            Labels = labels,
            Comments = comments.Select(c => new TaskCommentDto
            {
                Id = c.Id,
                CardId = c.CardId,
                AuthorUserId = c.AuthorUserId,
                AuthorName = userNames.GetValueOrDefault(c.AuthorUserId),
                Body = c.Body,
                CreatedAtUtc = c.CreatedAtUtc
            }).ToArray()
        };
    }

    private async Task ReplaceLabelsAsync(
        TaskCard card,
        IReadOnlyCollection<Guid>? labelIds,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.TaskCardLabels.Where(x => x.CardId == card.Id).ToListAsync(cancellationToken);
        dbContext.TaskCardLabels.RemoveRange(existing);

        if (labelIds is null || labelIds.Count == 0) return;

        var validIds = await dbContext.TaskLabels
            .AsNoTracking()
            .Where(x => x.BoardId == card.BoardId && x.TenantId == card.TenantId && labelIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var labelId in validIds.Distinct())
        {
            dbContext.TaskCardLabels.Add(new TaskCardLabel { CardId = card.Id, LabelId = labelId });
        }
    }

    private async Task ValidateAssigneeAsync(Guid tenantId, Guid? assigneeUserId, CancellationToken cancellationToken)
    {
        if (!assigneeUserId.HasValue) return;

        var ok = await dbContext.UserTenants.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId
                && x.UserId == assigneeUserId.Value
                && x.Status == "active"
                && x.User.Status == "active",
            cancellationToken);

        if (!ok)
        {
            throw new AppException(
                "taskboards.assigneeNotActive",
                "The selected user is not an active member of this workspace.",
                400);
        }
    }

    private async Task ValidateContactAsync(Guid tenantId, Guid? contactId, CancellationToken cancellationToken)
    {
        if (!contactId.HasValue) return;

        var ok = await dbContext.Contacts.AsNoTracking()
            .AnyAsync(x => x.Id == contactId.Value && x.TenantId == tenantId, cancellationToken);

        if (!ok)
        {
            throw new AppException("taskboards.contactNotFound", "Related potential was not found.", 400);
        }
    }

    private async Task<TaskBoard> RequireBoardAsync(Guid boardId, Guid tenantId, CancellationToken cancellationToken)
    {
        return await dbContext.TaskBoards
            .FirstOrDefaultAsync(x => x.Id == boardId && x.TenantId == tenantId, cancellationToken)
            ?? throw new AppException("taskboards.notFound", "Board not found.", 404);
    }

    private Guid RequireTenant() =>
        currentTenant.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

    private Guid RequireUser() =>
        currentUser.UserId ?? throw new InvalidOperationException("Authenticated user is required.");

    private static string RequireTitle(string? value, string code, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new AppException(code, message, 400);
        return value.Trim();
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizePriority(string? priority)
    {
        var value = string.IsNullOrWhiteSpace(priority) ? "normal" : priority.Trim().ToLowerInvariant();
        return value is "low" or "normal" or "high" or "urgent" ? value : "normal";
    }
}
