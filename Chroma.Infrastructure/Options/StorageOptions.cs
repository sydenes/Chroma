namespace Chroma.Infrastructure.Options;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; init; } = "Local";
    public string LocalPath { get; init; } = "storage";
    public int MaxFileSizeMb { get; init; } = 25;
}
