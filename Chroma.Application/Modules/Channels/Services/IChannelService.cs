using Chroma.Application.Modules.Channels.Dtos;

namespace Chroma.Application.Modules.Channels.Services;

public interface IChannelService
{
    Task<ChannelSearchResult> SearchAsync(ChannelSearchRequest request, CancellationToken cancellationToken);
    Task<ChannelDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ChannelDto> CreateAsync(CreateChannelRequest request, CancellationToken cancellationToken);
    Task<ChannelDto?> UpdateAsync(Guid id, UpdateChannelRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
