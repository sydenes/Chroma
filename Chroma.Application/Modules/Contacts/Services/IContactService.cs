using Chroma.Application.Modules.Contacts.Dtos;

namespace Chroma.Application.Modules.Contacts.Services;

public interface IContactService
{
    Task<ContactSearchResult> SearchAsync(ContactSearchRequest request, CancellationToken cancellationToken);
    Task<ContactDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ContactDto> CreateAsync(CreateContactRequest request, CancellationToken cancellationToken);
    Task<ContactDto?> UpdateAsync(Guid id, UpdateContactRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
