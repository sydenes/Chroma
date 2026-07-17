using Chroma.Application.Abstractions;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.BackgroundServices;

/// <summary>
/// Creates in-app notifications from due tasks and starting appointments (runs every minute).
/// </summary>
public sealed class ReminderNotificationBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReminderNotificationBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Reminder notification processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Reminder notification processor iteration failed.");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var now = DateTime.UtcNow;
        var appointmentWindowStart = now.AddMinutes(-1);

        var dueTasks = await dbContext.CrmTasks
            .Where(task =>
                task.Status == "pending"
                && task.DueAtUtc != null
                && task.DueAtUtc <= now
                && task.OwnerId != null)
            .OrderBy(task => task.DueAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var task in dueTasks)
        {
            await TryCreateReminderAsync(
                dbContext,
                task.TenantId,
                task.OwnerId!.Value,
                sourceType: "task",
                sourceId: task.Id,
                title: "Hatırlatıcı",
                body: BuildTaskBody(task),
                cancellationToken);
        }

        var startingAppointments = await dbContext.Appointments
            .Where(appointment =>
                appointment.Status == "scheduled"
                && appointment.StartsAtUtc <= now
                && appointment.StartsAtUtc > appointmentWindowStart
                && appointment.OwnerId != null)
            .OrderBy(appointment => appointment.StartsAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var appointment in startingAppointments)
        {
            await TryCreateReminderAsync(
                dbContext,
                appointment.TenantId,
                appointment.OwnerId!.Value,
                sourceType: "appointment",
                sourceId: appointment.Id,
                title: "Randevu başlıyor",
                body: appointment.Title,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string BuildTaskBody(CrmTask task)
    {
        if (!string.IsNullOrWhiteSpace(task.Description))
        {
            return $"{task.Title}: {task.Description.Trim()}";
        }

        return task.Title;
    }

    private static async Task TryCreateReminderAsync(
        IApplicationDbContext dbContext,
        Guid tenantId,
        Guid userId,
        string sourceType,
        Guid sourceId,
        string title,
        string body,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Notifications.AnyAsync(
            notification =>
                notification.UserId == userId
                && notification.SourceType == sourceType
                && notification.SourceId == sourceId,
            cancellationToken);

        if (exists)
        {
            return;
        }

        dbContext.Notifications.Add(new Notification
        {
            TenantId = tenantId,
            UserId = userId,
            Title = title,
            Body = body.Trim(),
            NotificationType = "reminder",
            SourceType = sourceType,
            SourceId = sourceId
        });
    }
}
