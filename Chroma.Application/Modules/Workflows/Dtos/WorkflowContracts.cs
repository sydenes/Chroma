namespace Chroma.Application.Modules.Workflows.Dtos;

public sealed class WorkflowTriggerDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid WorkflowId { get; init; }
    public string TriggerType { get; init; } = string.Empty;
    public string? ConfigJson { get; init; }
}

public sealed class WorkflowActionDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid WorkflowId { get; init; }
    public string ActionType { get; init; } = string.Empty;
    public string? ConfigJson { get; init; }
    public int Order { get; init; }
}

public sealed class WorkflowConditionDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid WorkflowId { get; init; }
    public string ConditionType { get; init; } = string.Empty;
    public string? ConfigJson { get; init; }
    public int Order { get; init; }
}

public sealed class WorkflowDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyCollection<WorkflowTriggerDto> Triggers { get; init; } = [];
    public IReadOnlyCollection<WorkflowConditionDto> Conditions { get; init; } = [];
    public IReadOnlyCollection<WorkflowActionDto> Actions { get; init; } = [];
}

public sealed class WorkflowSearchRequest
{
    public Guid TenantId { get; init; }
    public string? Query { get; init; }
    public bool? IsActive { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class WorkflowSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<WorkflowDto> Items { get; init; } = [];
}

public sealed class CreateWorkflowRequest
{
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IReadOnlyCollection<CreateWorkflowTriggerRequest> Triggers { get; init; } = [];
    public IReadOnlyCollection<CreateWorkflowConditionRequest> Conditions { get; init; } = [];
    public IReadOnlyCollection<CreateWorkflowActionRequest> Actions { get; init; } = [];
}

public sealed class UpdateWorkflowRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CreateWorkflowTriggerRequest
{
    public string TriggerType { get; init; } = string.Empty;
    public string? ConfigJson { get; init; }
}

public sealed class CreateWorkflowConditionRequest
{
    public string ConditionType { get; init; } = string.Empty;
    public string? ConfigJson { get; init; }
    public int Order { get; init; }
}

public sealed class CreateWorkflowActionRequest
{
    public string ActionType { get; init; } = string.Empty;
    public string? ConfigJson { get; init; }
    public int Order { get; init; }
}
