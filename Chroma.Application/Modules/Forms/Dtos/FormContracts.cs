namespace Chroma.Application.Modules.Forms.Dtos;

public sealed class FormFieldDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid FormId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string FieldType { get; init; } = "text";
    public bool IsRequired { get; init; }
    public int Order { get; init; }
    public string? OptionsJson { get; init; }
}

public sealed class FormDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public bool CreateContactOnSubmit { get; init; }
    public IReadOnlyCollection<FormFieldDto> Fields { get; init; } = [];
}

public sealed class FormSearchRequest
{
    public Guid TenantId { get; init; }
    public string? Query { get; init; }
    public bool? IsActive { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class FormSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<FormDto> Items { get; init; } = [];
}

public sealed class CreateFormRequest
{
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool CreateContactOnSubmit { get; init; }
    public IReadOnlyCollection<CreateFormFieldRequest> Fields { get; init; } = [];
}

public sealed class UpdateFormRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public bool CreateContactOnSubmit { get; init; }
}

public sealed class CreateFormFieldRequest
{
    public string Name { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string FieldType { get; init; } = "text";
    public bool IsRequired { get; init; }
    public int Order { get; init; }
    public string? OptionsJson { get; init; }
}

public sealed class UpdateFormFieldRequest
{
    public string Name { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string FieldType { get; init; } = "text";
    public bool IsRequired { get; init; }
    public int Order { get; init; }
    public string? OptionsJson { get; init; }
}

public sealed class FormResponseDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid FormId { get; init; }
    public Guid? ContactId { get; init; }
    public string JsonData { get; init; } = "{}";
}

public sealed class SubmitFormResponseRequest
{
    public Guid TenantId { get; init; }
    public string JsonData { get; init; } = "{}";
    public string? ContactFirstName { get; init; }
    public string? ContactLastName { get; init; }
}
