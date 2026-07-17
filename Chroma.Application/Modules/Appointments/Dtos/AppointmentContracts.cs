namespace Chroma.Application.Modules.Appointments.Dtos;

public sealed class AppointmentDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid? ContactId { get; init; }
    public string? ContactName { get; set; }
    public Guid? OwnerId { get; init; }
    public string? OwnerName { get; set; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime StartsAtUtc { get; init; }
    public DateTime EndsAtUtc { get; init; }
    public string Status { get; init; } = "scheduled";
    public string Mode { get; init; } = "office";
    public string SessionType { get; init; } = "follow_up";
    public string? SessionSummary { get; init; }
    public string? PrivateNotes { get; init; }
    public string? NextSteps { get; init; }
    public int? ProgressScore { get; init; }
}

public sealed class AppointmentSearchRequest
{
    public Guid TenantId { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? OwnerId { get; init; }
    public string? Status { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class AppointmentSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<AppointmentDto> Items { get; init; } = [];
}

public sealed class CreateAppointmentRequest
{
    public Guid? ContactId { get; init; }
    public Guid? OwnerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime StartsAtUtc { get; init; }
    public DateTime EndsAtUtc { get; init; }
    public string Mode { get; init; } = "office";
    public string SessionType { get; init; } = "follow_up";
    public string? SessionSummary { get; init; }
    public string? PrivateNotes { get; init; }
    public string? NextSteps { get; init; }
    public int? ProgressScore { get; init; }
}

public sealed class UpdateAppointmentRequest
{
    public Guid? ContactId { get; init; }
    public Guid? OwnerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime StartsAtUtc { get; init; }
    public DateTime EndsAtUtc { get; init; }
    public string Status { get; init; } = "scheduled";
    public string Mode { get; init; } = "office";
    public string SessionType { get; init; } = "follow_up";
    public string? SessionSummary { get; init; }
    public string? PrivateNotes { get; init; }
    public string? NextSteps { get; init; }
    public int? ProgressScore { get; init; }
}
