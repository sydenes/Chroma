namespace Chroma.Application.Modules.Contacts.Dtos;

public sealed class ContactChannelDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid ContactId { get; init; }
    public string ChannelType { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public bool Verified { get; init; }
}

public sealed class ContactChannelSearchRequest
{
    public Guid ContactId { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class ContactChannelSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<ContactChannelDto> Items { get; init; } = [];
}

public sealed class CreateContactChannelRequest
{
    public Guid TenantId { get; init; }
    public Guid ContactId { get; init; }
    public string ChannelType { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}

public sealed class UpdateContactChannelRequest
{
    public string ChannelType { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public bool Verified { get; init; }
}
