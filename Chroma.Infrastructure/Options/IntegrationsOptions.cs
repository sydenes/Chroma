namespace Chroma.Infrastructure.Options;

public sealed class IntegrationsOptions
{
    public const string SectionName = "Integrations";

    public string ChannelSecretKey { get; init; } = string.Empty;
    public WhatsAppIntegrationOptions WhatsApp { get; init; } = new();
    public MetaIntegrationOptions Meta { get; init; } = new();
    public EmailIntegrationOptions Email { get; init; } = new();
    public TelegramIntegrationOptions Telegram { get; init; } = new();
}

public sealed class WhatsAppIntegrationOptions
{
    public string ApiVersion { get; init; } = "v18.0";
}

public sealed class MetaIntegrationOptions
{
    public string ApiVersion { get; init; } = "v18.0";
}

public sealed class EmailIntegrationOptions
{
    public string SmtpHost { get; init; } = "localhost";
    public int SmtpPort { get; init; } = 587;
}

public sealed class TelegramIntegrationOptions
{
    public string ApiBaseUrl { get; init; } = "https://api.telegram.org";
}
