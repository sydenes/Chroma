using Chroma.Application.Abstractions;
using Chroma.Application.Common.Exceptions;
using Chroma.Application.Modules.Files.Dtos;
using Chroma.Application.Modules.Files.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class FileService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    IFileStorage fileStorage) : IFileService
{
    private const string LocalProvider = "local";
    private const long MaxUploadBytes = 20 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain"
    };

    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "photo", "document", "lab", "consent", "invoice", "avatar", "other"
    };

    public async Task<FileSearchResult> SearchAsync(FileSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.StoredFiles.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.OwnerType))
        {
            queryable = queryable.Where(x => x.OwnerType == request.OwnerType.Trim());
        }

        if (request.OwnerId.HasValue)
        {
            queryable = queryable.Where(x => x.OwnerId == request.OwnerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            queryable = queryable.Where(x => x.Category == request.Category.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queryable = queryable.Where(x => x.FileName.Contains(request.Query.Trim()));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        return new FileSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<FileDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.StoredFiles
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FileDto> UploadAsync(UploadFileRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        ValidateUpload(request);

        var fileId = Guid.NewGuid();
        var safeName = SanitizeFileName(request.FileName);
        var extension = Path.GetExtension(safeName);
        var category = NormalizeCategory(request.Category, request.ContentType);
        var storageKey =
            $"uploads/{tenantId:D}/{request.OwnerType.Trim().ToLowerInvariant()}/{request.OwnerId:D}/{fileId:D}{extension}";

        await fileStorage.SaveAsync(storageKey, request.Content, cancellationToken);

        var entity = new StoredFile
        {
            Id = fileId,
            TenantId = tenantId,
            OwnerType = request.OwnerType.Trim().ToLowerInvariant(),
            OwnerId = request.OwnerId,
            FileName = safeName,
            ContentType = request.ContentType.Trim(),
            SizeBytes = request.SizeBytes,
            Category = category,
            StorageProvider = LocalProvider,
            StorageKey = storageKey,
            Url = $"/api/files/{fileId:D}/download",
            UploadedByUserId = currentUser.UserId
        };

        dbContext.StoredFiles.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<FileDto> CreateAsync(CreateFileRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        if (string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.OwnerType))
        {
            throw new AppException(
                "files.fileNameAndOwnerRequired",
                "File name and owner are required.",
                400);
        }

        var entity = new StoredFile
        {
            TenantId = tenantId,
            OwnerType = request.OwnerType.Trim().ToLowerInvariant(),
            OwnerId = request.OwnerId,
            FileName = SanitizeFileName(request.FileName),
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType.Trim(),
            SizeBytes = request.SizeBytes,
            Category = NormalizeCategory(request.Category, request.ContentType),
            StorageProvider = string.IsNullOrWhiteSpace(request.StorageProvider)
                ? LocalProvider
                : request.StorageProvider.Trim().ToLowerInvariant(),
            StorageKey = request.Url.Trim(),
            Url = request.Url.Trim(),
            UploadedByUserId = currentUser.UserId
        };

        dbContext.StoredFiles.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        entity.Url = $"/api/files/{entity.Id:D}/download";
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<FileDto?> UpdateAsync(Guid id, UpdateFileRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.StoredFiles.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.FileName = SanitizeFileName(request.FileName);
        entity.OwnerType = request.OwnerType.Trim().ToLowerInvariant();
        entity.OwnerId = request.OwnerId;
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            entity.Category = NormalizeCategory(request.Category, entity.ContentType);
        }

        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<FileDownloadResult?> OpenDownloadAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.StoredFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var key = !string.IsNullOrWhiteSpace(entity.StorageKey) ? entity.StorageKey : entity.Url.TrimStart('/');
        var stream = await fileStorage.OpenReadAsync(key, cancellationToken);
        if (stream is null)
        {
            return null;
        }

        return new FileDownloadResult
        {
            Stream = stream,
            FileName = entity.FileName,
            ContentType = string.IsNullOrWhiteSpace(entity.ContentType)
                ? "application/octet-stream"
                : entity.ContentType
        };
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.StoredFiles.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var key = !string.IsNullOrWhiteSpace(entity.StorageKey) ? entity.StorageKey : entity.Url.TrimStart('/');
        if (!string.IsNullOrWhiteSpace(key))
        {
            try
            {
                await fileStorage.DeleteAsync(key, cancellationToken);
            }
            catch
            {
                // Soft-delete metadata even if physical delete fails.
            }
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ValidateUpload(UploadFileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerType) || request.OwnerId == Guid.Empty)
        {
            throw new AppException(
                "files.ownerRequired",
                "Owner type and owner id are required.",
                400);
        }

        if (string.IsNullOrWhiteSpace(request.FileName) || request.SizeBytes <= 0)
        {
            throw new AppException(
                "files.fileRequired",
                "A valid file is required.",
                400);
        }

        if (request.SizeBytes > MaxUploadBytes)
        {
            throw new AppException(
                "files.tooLarge",
                "File exceeds the 20 MB size limit.",
                400);
        }

        var contentType = request.ContentType?.Trim() ?? string.Empty;
        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new AppException(
                "files.contentTypeNotAllowed",
                "This file type is not allowed.",
                400);
        }
    }

    private static string NormalizeCategory(string? category, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalized = category.Trim().ToLowerInvariant();
            return AllowedCategories.Contains(normalized) ? normalized : "other";
        }

        return contentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true
            ? "photo"
            : "document";
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName.Trim());
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalid, '_');
        }

        return string.IsNullOrWhiteSpace(name) ? "file" : name;
    }

    private static FileDto ToDto(StoredFile entity)
    {
        return new FileDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            OwnerType = entity.OwnerType,
            OwnerId = entity.OwnerId,
            FileName = entity.FileName,
            ContentType = entity.ContentType,
            SizeBytes = entity.SizeBytes,
            Category = entity.Category,
            StorageProvider = entity.StorageProvider,
            StorageKey = entity.StorageKey,
            Url = entity.Url,
            UploadedByUserId = entity.UploadedByUserId,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    private static Expression<Func<StoredFile, FileDto>> MapToDto()
    {
        return x => new FileDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            OwnerType = x.OwnerType,
            OwnerId = x.OwnerId,
            FileName = x.FileName,
            ContentType = x.ContentType,
            SizeBytes = x.SizeBytes,
            Category = x.Category,
            StorageProvider = x.StorageProvider,
            StorageKey = x.StorageKey,
            Url = x.Url,
            UploadedByUserId = x.UploadedByUserId,
            CreatedAtUtc = x.CreatedAtUtc
        };
    }
}
