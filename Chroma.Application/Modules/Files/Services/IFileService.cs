using Chroma.Application.Modules.Files.Dtos;

namespace Chroma.Application.Modules.Files.Services;

public interface IFileService
{
    Task<FileSearchResult> SearchAsync(FileSearchRequest request, CancellationToken cancellationToken);
    Task<FileDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<FileDto> UploadAsync(UploadFileRequest request, CancellationToken cancellationToken);
    Task<FileDto> CreateAsync(CreateFileRequest request, CancellationToken cancellationToken);
    Task<FileDto?> UpdateAsync(Guid id, UpdateFileRequest request, CancellationToken cancellationToken);
    Task<FileDownloadResult?> OpenDownloadAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
