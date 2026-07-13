namespace Chroma.Application.Abstractions;

public interface IChannelAdapter
{
    string Provider { get; }

    Task<bool> ValidateConfigurationAsync(string settingsJson, CancellationToken cancellationToken = default);

    Task<ChannelSendResult> SendOutboundMessageAsync(
        ChannelOutboundMessage message,
        CancellationToken cancellationToken = default);
}

public sealed record ChannelOutboundMessage(
    Guid TenantId,
    Guid ChannelId,
    string RecipientId,
    string Content,
    string? MetadataJson = null);

public sealed record ChannelSendResult(bool Success, string? ExternalMessageId = null, string? Error = null);
