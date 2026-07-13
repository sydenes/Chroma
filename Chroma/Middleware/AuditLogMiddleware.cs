using System.Text;
using Chroma.Application.Abstractions;

namespace Chroma.Middleware;

public sealed class AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
{
    private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Delete
    };

    public async Task InvokeAsync(HttpContext context, IAuditService auditService, ICurrentUser currentUser)
    {
        if (!MutatingMethods.Contains(context.Request.Method) || !ShouldAudit(context.Request.Path))
        {
            await next(context);
            return;
        }

        string? requestBody = null;
        if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync(context.RequestAborted);
            context.Request.Body.Position = 0;
        }

        await next(context);

        if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
            return;
        }

        if (!currentUser.TenantId.HasValue)
        {
            return;
        }

        var (entity, entityId) = ParseRoute(context.Request.Path);

        try
        {
            await auditService.LogAsync(
                new AuditLogEntry(
                    currentUser.TenantId.Value,
                    currentUser.UserId,
                    entity,
                    entityId,
                    context.Request.Method.ToUpperInvariant(),
                    OldValueJson: null,
                    NewValueJson: requestBody),
                context.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write audit log for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
    }

    private static bool ShouldAudit(PathString path)
    {
        return path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }

    private static (string Entity, Guid EntityId) ParseRoute(PathString path)
    {
        var segments = path.Value?
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

        var entity = segments.Length >= 2 ? segments[1] : "unknown";
        var entityId = segments.Length >= 3 && Guid.TryParse(segments[2], out var parsedId)
            ? parsedId
            : Guid.Empty;

        return (entity, entityId);
    }
}
