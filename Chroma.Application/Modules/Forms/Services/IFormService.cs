using Chroma.Application.Modules.Forms.Dtos;

namespace Chroma.Application.Modules.Forms.Services;

public interface IFormService
{
    Task<FormSearchResult> SearchAsync(FormSearchRequest request, CancellationToken cancellationToken);
    Task<FormDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<FormDto> CreateAsync(CreateFormRequest request, CancellationToken cancellationToken);
    Task<FormDto?> UpdateAsync(Guid id, UpdateFormRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<FormFieldDto> CreateFieldAsync(Guid formId, CreateFormFieldRequest request, CancellationToken cancellationToken);
    Task<FormFieldDto?> UpdateFieldAsync(Guid formId, Guid fieldId, UpdateFormFieldRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteFieldAsync(Guid formId, Guid fieldId, CancellationToken cancellationToken);
    Task<FormResponseDto> SubmitResponseAsync(Guid formId, SubmitFormResponseRequest request, CancellationToken cancellationToken);
}
