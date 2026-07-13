namespace Chroma.Infrastructure.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string TenantSlug { get; init; } = "demo";
    public string TenantName { get; init; } = "Demo Company";
    public string AdminEmail { get; init; } = "admin@demo.local";
    public string AdminPassword { get; init; } = "Admin123!";
    public string AdminFirstName { get; init; } = "Admin";
    public string AdminLastName { get; init; } = "User";
}
