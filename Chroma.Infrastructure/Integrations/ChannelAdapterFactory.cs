using Chroma.Application.Abstractions;
using Chroma.Domain.Enums;

namespace Chroma.Infrastructure.Integrations;

public sealed class ChannelAdapterFactory(IEnumerable<IChannelAdapter> adapters) : IChannelAdapterFactory
{
    private readonly IReadOnlyDictionary<string, IChannelAdapter> _adapters = adapters
        .ToDictionary(adapter => adapter.Provider.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase);

    public IChannelAdapter GetAdapter(string provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);

        var normalized = NormalizeProvider(provider);

        if (_adapters.TryGetValue(normalized, out var adapter))
        {
            return adapter;
        }

        throw new NotSupportedException($"No channel adapter registered for provider '{provider}'.");
    }

    private static string NormalizeProvider(string provider)
    {
        var normalized = provider.ToUpperInvariant();

        return normalized switch
        {
            ChannelProvider.Facebook or ChannelProvider.Instagram => MetaAdapter.ProviderName,
            _ => normalized
        };
    }
}
