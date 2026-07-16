using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public string Status { get; set; } = "active";
}
