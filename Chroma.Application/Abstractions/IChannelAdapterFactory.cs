namespace Chroma.Application.Abstractions;

public interface IChannelAdapterFactory
{
    IChannelAdapter GetAdapter(string provider);
}
