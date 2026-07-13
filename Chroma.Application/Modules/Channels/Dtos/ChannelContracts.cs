namespace Chroma.Application.Modules.Channels.Dtos;

public sealed class ChannelDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? ExternalAccountId { get; init; }
    public string? SettingsJson { get; init; }
    public bool IsActive { get; init; }
}

public sealed class ChannelSearchRequest
{
    public Guid TenantId { get; init; }
    public string? Provider { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class ChannelSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<ChannelDto> Items { get; init; } = [];
}

public sealed class CreateChannelRequest
{
    public Guid TenantId { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? ExternalAccountId { get; init; }
    public string? SettingsJson { get; init; }
}

public sealed class UpdateChannelRequest
{
    public string Name { get; init; } = string.Empty;
    public string? ExternalAccountId { get; init; }
    public string? SettingsJson { get; init; }
    public bool IsActive { get; init; }
}
