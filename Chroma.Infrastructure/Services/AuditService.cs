using Chroma.Application.Abstractions;
using Chroma.Domain.Entities;

namespace Chroma.Infrastructure.Services;

public sealed class AuditService(IApplicationDbContext dbContext) : IAuditService
{
    public async Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            TenantId = entry.TenantId,
            UserId = entry.UserId,
            Entity = entry.Entity,
            EntityId = entry.EntityId,
            Action = entry.Action,
            OldValueJson = entry.OldValueJson,
            NewValueJson = entry.NewValueJson
        };

        dbContext.AuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
