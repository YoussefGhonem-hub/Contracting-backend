using Contracting.Domain.Entities.client;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.ThreeDDtos;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Features.client;

public class ThreeDFolderService : IThreeDFolderService
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storage;

    public ThreeDFolderService(ApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    // ── Authorization ─────────────────────────────────────────────────────────

    private async Task<bool> IsProjectManagerAsync(Guid projectId, CancellationToken ct)
    {
        var roles = CurrentUser.Roles;
        var isSuperOrAdmin = roles.Any(r => r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase)
                                          || r.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));
        if (isSuperOrAdmin) return true;

        if (!Guid.TryParse(CurrentUser.UserId, out var userId)) return false;

        return await _db.EngineerProjects
            .AnyAsync(ep => ep.ProjectId == projectId
                         && ep.Engineer!.ApplicationUserId == userId
                         && ep.IsProjectManager
                         && !ep.IsDeleted, ct);
    }

    // ── Engineer (Project Manager) operations ─────────────────────────────────

    public async Task<ErrorOr<GetThreeDFolderDto>> CreateFolderAsync(
        Guid projectId, CreateThreeDFolderDto dto, CancellationToken ct = default)
    {
        if (!await IsProjectManagerAsync(projectId, ct))
            return Error.Forbidden("ThreeD.Forbidden", "Only the project manager can manage 3D folders.");

        if (!Guid.TryParse(CurrentUser.UserId, out var userId))
            return Error.Unauthorized("Auth.Unauthorized", "User is not authenticated.");

        var projectExists = await _db.Projects.AnyAsync(p => p.Id == projectId && !p.IsDeleted, ct);
        if (!projectExists)
            return Error.NotFound("ThreeD.ProjectNotFound", "Project not found.");

        var folder = new ThreeDFolder
        {
            ProjectId  = projectId,
            Title      = dto.Title,
            UploadedBy = userId
        };

        if (dto.CoverImage != null)
        {
            var uploaded = await _storage.Upload(dto.CoverImage, ct);
            if (uploaded != null)
            {
                folder.CoverKey = uploaded.Key;
                folder.CoverUrl = uploaded.Url;
            }
        }

        _db.ThreeDFolders.Add(folder);
        await _db.SaveChangesAsync(ct);

        return MapFolderDto(folder, 0);
    }

    public async Task<ErrorOr<GetThreeDFolderDto>> UpdateFolderAsync(
        Guid projectId, Guid folderId, UpdateThreeDFolderDto dto, CancellationToken ct = default)
    {
        if (!await IsProjectManagerAsync(projectId, ct))
            return Error.Forbidden("ThreeD.Forbidden", "Only the project manager can manage 3D folders.");

        var folder = await _db.ThreeDFolders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.ProjectId == projectId && !f.IsDeleted, ct);

        if (folder is null)
            return Error.NotFound("ThreeD.FolderNotFound", "Folder not found.");

        if (!string.IsNullOrWhiteSpace(dto.Title))
            folder.Title = dto.Title;

        if (dto.CoverImage != null)
        {
            if (!string.IsNullOrEmpty(folder.CoverKey))
                await _storage.Delete(folder.CoverKey, ct);

            var uploaded = await _storage.Upload(dto.CoverImage, ct);
            if (uploaded != null)
            {
                folder.CoverKey = uploaded.Key;
                folder.CoverUrl = uploaded.Url;
            }
        }

        await _db.SaveChangesAsync(ct);

        var imageCount = await _db.ThreeDImages.CountAsync(i => i.FolderId == folderId && !i.IsDeleted, ct);
        return MapFolderDto(folder, imageCount);
    }

    public async Task<ErrorOr<bool>> DeleteFolderAsync(
        Guid projectId, Guid folderId, CancellationToken ct = default)
    {
        if (!await IsProjectManagerAsync(projectId, ct))
            return Error.Forbidden("ThreeD.Forbidden", "Only the project manager can manage 3D folders.");

        if (!Guid.TryParse(CurrentUser.UserId, out var userId))
            return Error.Unauthorized("Auth.Unauthorized", "User is not authenticated.");

        var folder = await _db.ThreeDFolders
            .Include(f => f.Images)
            .FirstOrDefaultAsync(f => f.Id == folderId && f.ProjectId == projectId && !f.IsDeleted, ct);

        if (folder is null)
            return Error.NotFound("ThreeD.FolderNotFound", "Folder not found.");

        foreach (var img in folder.Images.Where(i => !string.IsNullOrEmpty(i.Key)))
            await _storage.Delete(img.Key!, ct);

        if (!string.IsNullOrEmpty(folder.CoverKey))
            await _storage.Delete(folder.CoverKey, ct);

        folder.MarkAsDeleted(userId);
        foreach (var img in folder.Images)
            img.MarkAsDeleted(userId);

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ErrorOr<List<GetThreeDImageDto>>> AddImagesAsync(
        Guid projectId, Guid folderId, List<IFormFile> files, CancellationToken ct = default)
    {
        if (!await IsProjectManagerAsync(projectId, ct))
            return Error.Forbidden("ThreeD.Forbidden", "Only the project manager can manage 3D folders.");

        if (!Guid.TryParse(CurrentUser.UserId, out var userId))
            return Error.Unauthorized("Auth.Unauthorized", "User is not authenticated.");

        if (files == null || files.Count == 0)
            return Error.Validation("ThreeD.NoFiles", "At least one file is required.");

        var folderExists = await _db.ThreeDFolders
            .AnyAsync(f => f.Id == folderId && f.ProjectId == projectId && !f.IsDeleted, ct);

        if (!folderExists)
            return Error.NotFound("ThreeD.FolderNotFound", "Folder not found.");

        var result = new List<GetThreeDImageDto>();

        foreach (var file in files)
        {
            var uploaded = await _storage.Upload(file, ct);
            if (uploaded is null) continue;

            var image = new ThreeDImage
            {
                FolderId   = folderId,
                Key        = uploaded.Key,
                FileName   = file.FileName,
                Extension  = Path.GetExtension(file.FileName).ToLowerInvariant(),
                FileSize   = file.Length,
                Url        = uploaded.Url,
                UploadedBy = userId
            };

            _db.ThreeDImages.Add(image);
            result.Add(new GetThreeDImageDto
            {
                Id         = image.Id,
                FileName   = image.FileName,
                Extension  = image.Extension,
                FileSize   = image.FileSize,
                Url        = _storage.GetPreSignedUrl(image.Key) ?? image.Url,
                UploadedAt = image.CreatedDate
            });
        }

        await _db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<ErrorOr<bool>> DeleteImageAsync(
        Guid projectId, Guid folderId, Guid imageId, CancellationToken ct = default)
    {
        if (!await IsProjectManagerAsync(projectId, ct))
            return Error.Forbidden("ThreeD.Forbidden", "Only the project manager can manage 3D folders.");

        if (!Guid.TryParse(CurrentUser.UserId, out var userId))
            return Error.Unauthorized("Auth.Unauthorized", "User is not authenticated.");

        var image = await _db.ThreeDImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.FolderId == folderId && !i.IsDeleted, ct);

        if (image is null)
            return Error.NotFound("ThreeD.ImageNotFound", "Image not found.");

        if (!string.IsNullOrEmpty(image.Key))
            await _storage.Delete(image.Key, ct);

        image.MarkAsDeleted(userId);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ── Read (client + backoffice + engineers) ────────────────────────────────

    public async Task<ErrorOr<List<GetThreeDFolderDto>>> GetFoldersAsync(
        Guid projectId, CancellationToken ct = default)
    {
        var folders = await _db.ThreeDFolders
            .Where(f => f.ProjectId == projectId && !f.IsDeleted)
            .OrderByDescending(f => f.ModifiedDate ?? f.CreatedDate)
            .Select(f => new
            {
                f.Id, f.Title, f.CoverKey, f.CoverUrl,
                f.ModifiedDate, f.CreatedDate,
                ImageCount = f.Images.Count(i => !i.IsDeleted)
            })
            .ToListAsync(ct);

        return folders.Select(f => new GetThreeDFolderDto
        {
            Id         = f.Id,
            Title      = f.Title,
            CoverUrl   = _storage.GetPreSignedUrl(f.CoverKey) ?? f.CoverUrl,
            ImageCount = f.ImageCount,
            UpdatedAt  = f.ModifiedDate ?? f.CreatedDate
        }).ToList();
    }

    public async Task<ErrorOr<GetThreeDFolderDetailDto>> GetFolderDetailAsync(
        Guid projectId, Guid folderId, CancellationToken ct = default)
    {
        var folder = await _db.ThreeDFolders
            .Include(f => f.Images.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(f => f.Id == folderId && f.ProjectId == projectId && !f.IsDeleted, ct);

        if (folder is null)
            return Error.NotFound("ThreeD.FolderNotFound", "Folder not found.");

        return new GetThreeDFolderDetailDto
        {
            Id         = folder.Id,
            Title      = folder.Title,
            CoverUrl   = _storage.GetPreSignedUrl(folder.CoverKey) ?? folder.CoverUrl,
            ImageCount = folder.Images.Count,
            UpdatedAt  = folder.ModifiedDate ?? folder.CreatedDate,
            Images     = folder.Images.Select(i => new GetThreeDImageDto
            {
                Id         = i.Id,
                FileName   = i.FileName,
                Extension  = i.Extension,
                FileSize   = i.FileSize,
                Url        = _storage.GetPreSignedUrl(i.Key) ?? i.Url,
                UploadedAt = i.CreatedDate
            }).ToList()
        };
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private GetThreeDFolderDto MapFolderDto(ThreeDFolder f, int imageCount) => new()
    {
        Id         = f.Id,
        Title      = f.Title,
        CoverUrl   = _storage.GetPreSignedUrl(f.CoverKey) ?? f.CoverUrl,
        ImageCount = imageCount,
        UpdatedAt  = f.ModifiedDate ?? f.CreatedDate
    };
}
