using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

/// <summary>
/// Tenant müşterisinin potansiyel müşterisi (danışan, hasta, müvekkil vb.).
/// Tablo adı geriye uyumluluk için contacts kalır.
/// </summary>
public class Contact : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
    public string? Source { get; set; }
    /// <summary>active | inactive</summary>
    public string Status { get; set; } = "active";
    /// <summary>Tenant tipine göre: danisan, hasta, muvvekkil, musteri, lead...</summary>
    public string PotentialType { get; set; } = "lead";
    /// <summary>new | contacted | qualified | active | paused | won | lost</summary>
    public string LifecycleStage { get; set; } = "new";
    public decimal? EstimatedValue { get; set; }
    public string Currency { get; set; } = "TRY";
}
