using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class StoredFile : BaseEntity
{
    public Guid TenantId { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StorageProvider { get; set; } = "local";
    public string Url { get; set; } = string.Empty;
}
