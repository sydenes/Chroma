using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class ContactChannel : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid ContactId { get; set; }
    public string ChannelType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool Verified { get; set; }
}
