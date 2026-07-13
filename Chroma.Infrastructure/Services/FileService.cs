using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Files.Dtos;
using Chroma.Application.Modules.Files.Services;
using Chroma.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class FileService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant,
    IWebHostEnvironment webHostEnvironment) : IFileService
{
    private const string LocalProvider = "local";
    private const string UploadsFolder = "uploads";

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

    public async Task<FileDto> CreateAsync(CreateFileRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var url = request.Url;
        var storageProvider = string.IsNullOrWhiteSpace(request.StorageProvider) ? LocalProvider : request.StorageProvider.Trim().ToLowerInvariant();

        if (storageProvider == LocalProvider)
        {
            var tenantUploadPath = Path.Combine(webHostEnvironment.WebRootPath, UploadsFolder, tenantId.ToString());
            Directory.CreateDirectory(tenantUploadPath);

            if (string.IsNullOrWhiteSpace(url))
            {
                url = $"/{UploadsFolder}/{tenantId}/{request.FileName.Trim()}";
            }
            else if (!url.StartsWith('/'))
            {
                url = $"/{UploadsFolder}/{tenantId}/{url.TrimStart('/')}";
            }
        }

        var entity = new StoredFile
        {
            TenantId = tenantId,
            OwnerType = request.OwnerType.Trim(),
            OwnerId = request.OwnerId,
            FileName = request.FileName.Trim(),
            ContentType = request.ContentType,
            SizeBytes = request.SizeBytes,
            StorageProvider = storageProvider,
            Url = url
        };

        dbContext.StoredFiles.Add(entity);
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

        entity.FileName = request.FileName.Trim();
        entity.OwnerType = request.OwnerType.Trim();
        entity.OwnerId = request.OwnerId;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
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

        if (entity.StorageProvider == LocalProvider && !string.IsNullOrWhiteSpace(entity.Url))
        {
            var relativePath = entity.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(webHostEnvironment.WebRootPath, relativePath);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
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
            StorageProvider = entity.StorageProvider,
            Url = entity.Url
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
            StorageProvider = x.StorageProvider,
            Url = x.Url
        };
    }
}
