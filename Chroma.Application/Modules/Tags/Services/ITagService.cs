using Chroma.Application.Modules.Tags.Dtos;

namespace Chroma.Application.Modules.Tags.Services;

public interface ITagService
{
    Task<TagSearchResult> SearchAsync(TagSearchRequest request, CancellationToken cancellationToken);
    Task<TagDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TagDto> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken);
    Task<TagDto?> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> AssignToContactAsync(Guid tagId, Guid contactId, CancellationToken cancellationToken);
    Task<bool> AssignToCompanyAsync(Guid tagId, Guid companyId, CancellationToken cancellationToken);
    Task<bool> UnassignFromContactAsync(Guid tagId, Guid contactId, CancellationToken cancellationToken);
    Task<bool> UnassignFromCompanyAsync(Guid tagId, Guid companyId, CancellationToken cancellationToken);
}
