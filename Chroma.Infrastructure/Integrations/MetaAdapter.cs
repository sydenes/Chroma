using Chroma.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Chroma.Infrastructure.Integrations;

public sealed class MetaAdapter(ILogger<MetaAdapter> logger) : IChannelAdapter
{
    public const string ProviderName = "META";

    public string Provider => ProviderName;

    public Task<bool> ValidateConfigurationAsync(string settingsJson, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Validating Meta channel configuration (skeleton).");
        return Task.FromResult(!string.IsNullOrWhiteSpace(settingsJson));
    }

    public Task<ChannelSendResult> SendOutboundMessageAsync(
        ChannelOutboundMessage message,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Meta outbound message skeleton: Channel={ChannelId}, Recipient={RecipientId}",
            message.ChannelId,
            message.RecipientId);

        return Task.FromResult(new ChannelSendResult(true, ExternalMessageId: $"meta-{Guid.NewGuid():N}"));
    }
}
