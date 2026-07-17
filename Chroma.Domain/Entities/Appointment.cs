using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    /// <summary>Pre-session planning note.</summary>
    public string? Notes { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public string Status { get; set; } = "scheduled";
    public string Mode { get; set; } = "office";
    /// <summary>initial | follow_up | control | evaluation | other</summary>
    public string SessionType { get; set; } = "follow_up";
    /// <summary>Post-session clinical/operational report summary.</summary>
    public string? SessionSummary { get; set; }
    /// <summary>Staff-only private notes.</summary>
    public string? PrivateNotes { get; set; }
    /// <summary>Recommended next actions / homework.</summary>
    public string? NextSteps { get; set; }
    /// <summary>Progress score 1-5 after session.</summary>
    public int? ProgressScore { get; set; }
}
