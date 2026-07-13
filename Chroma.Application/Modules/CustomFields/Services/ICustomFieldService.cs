using Chroma.Application.Modules.CustomFields.Dtos;

namespace Chroma.Application.Modules.CustomFields.Services;

public interface ICustomFieldService
{
    Task<CustomFieldSearchResult> SearchAsync(CustomFieldSearchRequest request, CancellationToken cancellationToken);
    Task<CustomFieldDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CustomFieldDto> CreateAsync(CreateCustomFieldRequest request, CancellationToken cancellationToken);
    Task<CustomFieldDto?> UpdateAsync(Guid id, UpdateCustomFieldRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CustomFieldValueDto>> GetValuesAsync(GetCustomFieldValuesRequest request, CancellationToken cancellationToken);
    Task<CustomFieldValueDto> SetValueAsync(SetCustomFieldValueRequest request, CancellationToken cancellationToken);
}
