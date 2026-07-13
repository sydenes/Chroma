using Chroma.Application.Abstractions;
using Chroma.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Chroma.Infrastructure.Integrations;

public sealed class TelegramAdapter(ILogger<TelegramAdapter> logger) : IChannelAdapter
{
    public string Provider => ChannelProvider.Telegram;

    public Task<bool> ValidateConfigurationAsync(string settingsJson, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Validating Telegram channel configuration (skeleton).");
        return Task.FromResult(!string.IsNullOrWhiteSpace(settingsJson));
    }

    public Task<ChannelSendResult> SendOutboundMessageAsync(
        ChannelOutboundMessage message,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Telegram outbound message skeleton: Channel={ChannelId}, Recipient={RecipientId}",
            message.ChannelId,
            message.RecipientId);

        return Task.FromResult(new ChannelSendResult(true, ExternalMessageId: $"tg-{Guid.NewGuid():N}"));
    }
}
