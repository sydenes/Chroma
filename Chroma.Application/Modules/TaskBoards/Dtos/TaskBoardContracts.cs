namespace Chroma.Application.Modules.TaskBoards.Dtos;

public sealed class TaskBoardDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public IReadOnlyCollection<TaskColumnDto> Columns { get; init; } = [];
    public IReadOnlyCollection<TaskLabelDto> Labels { get; init; } = [];
}

public sealed class TaskColumnDto
{
    public Guid Id { get; init; }
    public Guid BoardId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public string? Color { get; init; }
    public IReadOnlyCollection<TaskCardSummaryDto> Cards { get; set; } = [];
}

public sealed class TaskCardSummaryDto
{
    public Guid Id { get; init; }
    public Guid BoardId { get; init; }
    public Guid ColumnId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public Guid? AssigneeUserId { get; init; }
    public string? AssigneeName { get; set; }
    public Guid? ContactId { get; init; }
    public string? ContactName { get; set; }
    public string Priority { get; init; } = "normal";
    public DateTime? DueAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public IReadOnlyCollection<TaskLabelDto> Labels { get; set; } = [];
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }
}

public sealed class TaskCardDetailDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid BoardId { get; init; }
    public Guid ColumnId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string? CreatedByName { get; set; }
    public Guid? AssigneeUserId { get; init; }
    public string? AssigneeName { get; set; }
    public Guid? ContactId { get; init; }
    public string? ContactName { get; set; }
    public string Priority { get; init; } = "normal";
    public DateTime? DueAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public IReadOnlyCollection<TaskLabelDto> Labels { get; set; } = [];
    public IReadOnlyCollection<TaskCommentDto> Comments { get; set; } = [];
}

public sealed class TaskLabelDto
{
    public Guid Id { get; init; }
    public Guid BoardId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = "#64748b";
}

public sealed class TaskCommentDto
{
    public Guid Id { get; init; }
    public Guid CardId { get; init; }
    public Guid AuthorUserId { get; init; }
    public string? AuthorName { get; set; }
    public string Body { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class CreateTaskColumnRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
}

public sealed class UpdateTaskColumnRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
    public int? SortOrder { get; init; }
}

public sealed class CreateTaskCardRequest
{
    public Guid ColumnId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? AssigneeUserId { get; init; }
    public Guid? ContactId { get; init; }
    public string Priority { get; init; } = "normal";
    public DateTime? DueAtUtc { get; init; }
    public IReadOnlyCollection<Guid>? LabelIds { get; init; }
}

public sealed class UpdateTaskCardRequest
{
    public Guid ColumnId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? AssigneeUserId { get; init; }
    public Guid? ContactId { get; init; }
    public string Priority { get; init; } = "normal";
    public DateTime? DueAtUtc { get; init; }
    public IReadOnlyCollection<Guid>? LabelIds { get; init; }
}

public sealed class MoveTaskCardRequest
{
    public Guid ColumnId { get; init; }
    public int SortOrder { get; init; }
}

public sealed class CreateTaskLabelRequest
{
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = "#64748b";
}

public sealed class UpdateTaskLabelRequest
{
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = "#64748b";
}

public sealed class CreateTaskCommentRequest
{
    public string Body { get; init; } = string.Empty;
}
