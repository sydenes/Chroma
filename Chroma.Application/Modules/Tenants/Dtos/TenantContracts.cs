namespace Chroma.Application.Modules.Tenants.Dtos;

public sealed class TenantSettingsDto
{
    public Guid TenantId { get; init; }
    public string Theme { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public string? ExtrasJson { get; init; }
}

public sealed class UpdateTenantSettingsRequest
{
    public string Theme { get; init; } = "light";
    public string Language { get; init; } = "tr";
    public string Currency { get; init; } = "TRY";
    public string TimeZone { get; init; } = "UTC";
    public string? ExtrasJson { get; init; }
}
