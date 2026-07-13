namespace Chroma.Application.Modules.Notifications.Dtos;

public sealed class NotificationDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string NotificationType { get; init; } = "info";
    public bool IsRead { get; init; }
    public DateTime? ReadAtUtc { get; init; }
}

public sealed class NotificationSearchRequest
{
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public bool? IsRead { get; init; }
    public string? NotificationType { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class NotificationSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<NotificationDto> Items { get; init; } = [];
}

public sealed class CreateNotificationRequest
{
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string NotificationType { get; init; } = "info";
}

public sealed class MarkNotificationReadRequest
{
    public bool IsRead { get; init; } = true;
}
