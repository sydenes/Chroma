namespace Chroma.Application.Abstractions;

public interface IAuditService
{
    Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}

public sealed record AuditLogEntry(
    Guid TenantId,
    Guid? UserId,
    string Entity,
    Guid EntityId,
    string Action,
    string? OldValueJson = null,
    string? NewValueJson = null);
