using Chroma.Application.Modules.Contacts.Dtos;

namespace Chroma.Application.Modules.Contacts.Services;

public interface IContactChannelService
{
    Task<ContactChannelSearchResult> SearchAsync(ContactChannelSearchRequest request, CancellationToken cancellationToken);
    Task<ContactChannelDto?> GetByIdAsync(Guid contactId, Guid id, CancellationToken cancellationToken);
    Task<ContactChannelDto> CreateAsync(CreateContactChannelRequest request, CancellationToken cancellationToken);
    Task<ContactChannelDto?> UpdateAsync(Guid contactId, Guid id, UpdateContactChannelRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid contactId, Guid id, CancellationToken cancellationToken);
}
