namespace Chroma.Application.Modules.CustomFields.Dtos;

public sealed class CustomFieldDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string FieldType { get; init; } = "text";
    public string? SettingsJson { get; init; }
}

public sealed class CustomFieldValueDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid FieldId { get; init; }
    public Guid EntityId { get; init; }
    public string Value { get; init; } = string.Empty;
}

public sealed class CustomFieldSearchRequest
{
    public Guid TenantId { get; init; }
    public string? EntityType { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class CustomFieldSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<CustomFieldDto> Items { get; init; } = [];
}

public sealed class CreateCustomFieldRequest
{
    public Guid TenantId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string FieldType { get; init; } = "text";
    public string? SettingsJson { get; init; }
}

public sealed class UpdateCustomFieldRequest
{
    public string Name { get; init; } = string.Empty;
    public string FieldType { get; init; } = "text";
    public string? SettingsJson { get; init; }
}

public sealed class SetCustomFieldValueRequest
{
    public Guid TenantId { get; init; }
    public Guid FieldId { get; init; }
    public Guid EntityId { get; init; }
    public string Value { get; init; } = string.Empty;
}

public sealed class GetCustomFieldValuesRequest
{
    public Guid TenantId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
}
