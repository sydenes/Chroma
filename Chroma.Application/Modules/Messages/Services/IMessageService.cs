using Chroma.Application.Modules.Messages.Dtos;

namespace Chroma.Application.Modules.Messages.Services;

public interface IMessageService
{
    Task<MessageSearchResult> SearchAsync(MessageSearchRequest request, CancellationToken cancellationToken);
    Task<MessageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<MessageDto> SendOutboundAsync(SendOutboundMessageRequest request, CancellationToken cancellationToken);
}
