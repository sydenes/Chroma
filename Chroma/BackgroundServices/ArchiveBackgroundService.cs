namespace Chroma.BackgroundServices;

public sealed class ArchiveBackgroundService(ILogger<ArchiveBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Archive background service started (skeleton).");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ArchiveMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Archive background service iteration failed.");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }

    private Task ArchiveMessagesAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Message archive skeleton tick at {TimestampUtc}", DateTime.UtcNow);
        return Task.CompletedTask;
    }
}
