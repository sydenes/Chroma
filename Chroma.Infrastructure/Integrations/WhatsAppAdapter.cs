using Chroma.Application.Abstractions;
using Chroma.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Chroma.Infrastructure.Integrations;

public sealed class WhatsAppAdapter(ILogger<WhatsAppAdapter> logger) : IChannelAdapter
{
    public string Provider => ChannelProvider.WhatsApp;

    public Task<bool> ValidateConfigurationAsync(string settingsJson, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Validating WhatsApp channel configuration (skeleton).");
        return Task.FromResult(!string.IsNullOrWhiteSpace(settingsJson));
    }

    public Task<ChannelSendResult> SendOutboundMessageAsync(
        ChannelOutboundMessage message,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "WhatsApp outbound message skeleton: Channel={ChannelId}, Recipient={RecipientId}",
            message.ChannelId,
            message.RecipientId);

        return Task.FromResult(new ChannelSendResult(true, ExternalMessageId: $"wa-{Guid.NewGuid():N}"));
    }
}
