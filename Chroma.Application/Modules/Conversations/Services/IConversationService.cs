using Chroma.Application.Modules.Conversations.Dtos;

namespace Chroma.Application.Modules.Conversations.Services;

public interface IConversationService
{
    Task<ConversationSearchResult> SearchAsync(ConversationSearchRequest request, CancellationToken cancellationToken);
    Task<ConversationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ConversationDto> CreateAsync(CreateConversationRequest request, CancellationToken cancellationToken);
    Task<ConversationDto?> UpdateAsync(Guid id, UpdateConversationRequest request, CancellationToken cancellationToken);
    Task<ConversationDto?> AssignAsync(Guid id, AssignConversationRequest request, CancellationToken cancellationToken);
    Task<ConversationDto?> UpdateStatusAsync(Guid id, UpdateConversationStatusRequest request, CancellationToken cancellationToken);
    Task<ConversationDto?> MarkAsReadAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
