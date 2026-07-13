using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Permission : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }
}
