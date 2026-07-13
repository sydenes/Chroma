namespace Chroma.Application.Modules.Pipelines.Dtos;

public sealed class StageDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid PipelineId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
    public string? Color { get; init; }
    public bool IsWinStage { get; init; }
    public bool IsLostStage { get; init; }
}

public sealed class PipelineDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
    public IReadOnlyCollection<StageDto> Stages { get; init; } = [];
}

public sealed class PipelineSearchRequest
{
    public Guid TenantId { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class PipelineSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<PipelineDto> Items { get; init; } = [];
}

public sealed class CreatePipelineRequest
{
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
    public IReadOnlyCollection<CreateStageRequest> Stages { get; init; } = [];
}

public sealed class UpdatePipelineRequest
{
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
}

public sealed class CreateStageRequest
{
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
    public string? Color { get; init; }
    public bool IsWinStage { get; init; }
    public bool IsLostStage { get; init; }
}

public sealed class UpdateStageRequest
{
    public string Name { get; init; } = string.Empty;
    public int Order { get; init; }
    public string? Color { get; init; }
    public bool IsWinStage { get; init; }
    public bool IsLostStage { get; init; }
}
