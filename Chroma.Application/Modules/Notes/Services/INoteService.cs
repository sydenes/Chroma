using Chroma.Application.Modules.Notes.Dtos;

namespace Chroma.Application.Modules.Notes.Services;

public interface INoteService
{
    Task<NoteSearchResult> SearchAsync(NoteSearchRequest request, CancellationToken cancellationToken);
    Task<NoteDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<NoteDto> CreateAsync(CreateNoteRequest request, CancellationToken cancellationToken);
    Task<NoteDto?> UpdateAsync(Guid id, UpdateNoteRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
