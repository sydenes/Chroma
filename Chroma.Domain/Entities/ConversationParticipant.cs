using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class ConversationParticipant : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ContactId { get; set; }
    public string? ExternalParticipantId { get; set; }
    public string Role { get; set; } = "member";
}
