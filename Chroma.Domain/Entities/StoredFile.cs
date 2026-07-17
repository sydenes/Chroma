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
    /// <summary>photo | document | lab | consent | invoice | other</summary>
    public string Category { get; set; } = "document";
    public string StorageProvider { get; set; } = "local";
    /// <summary>Provider-relative storage key (path inside storage root).</summary>
    public string StorageKey { get; set; } = string.Empty;
    /// <summary>Legacy/display path; preferred access is /api/files/{id}/download.</summary>
    public string Url { get; set; } = string.Empty;
    public Guid? UploadedByUserId { get; set; }
}
