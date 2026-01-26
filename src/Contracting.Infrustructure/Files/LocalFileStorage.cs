using Contracting.Shared.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Contracting.Infrustructure.Files;

public class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveAsync(IFormFile file, string subFolder, CancellationToken ct)
    {
        var wwwroot = GetWebRoot();
        var folder = Path.Combine(wwwroot, subFolder);
        Directory.CreateDirectory(folder);

        var safeName = Path.GetFileNameWithoutExtension(file.FileName);
        var ext = Path.GetExtension(file.FileName);
        var unique = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, unique);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        // Return relative path used by static files middleware
        return Path.Combine(subFolder, unique).Replace("\\", "/");
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;

        var root = GetWebRoot();
        var fullPath = Path.Combine(root, relativePath.TrimStart('/', '\\'));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    public async Task DeleteManyAsync(IEnumerable<string> relativePaths, CancellationToken ct)
    {
        foreach (var path in relativePaths)
            await DeleteAsync(path, ct);
    }
    public async Task<string> SaveUserAvatarAsync(Guid userId, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var userFolder = userId.ToString("N");
        var uploadsRoot = Path.Combine(GetWebRoot(), "uploads", "users", userFolder);
        Directory.CreateDirectory(uploadsRoot);

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = GuessExtension(contentType);

        var finalName = $"avatar{ext}".ToLowerInvariant();
        var fullPath = Path.Combine(uploadsRoot, finalName);

        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fs, ct);

        var relative = $"/uploads/users/{userFolder}/{finalName}";
        return relative;
    }

    public async Task<string> SaveUserSignatureAsync(Guid userId, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var userFolder = userId.ToString("N");
        var uploadsRoot = Path.Combine(GetWebRoot(), "uploads", "users", userFolder, "signatures");
        Directory.CreateDirectory(uploadsRoot);

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = GuessExtension(contentType);

        var finalName = $"signature-{Guid.NewGuid():N}{ext}".ToLowerInvariant();
        var fullPath = Path.Combine(uploadsRoot, finalName);

        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fs, ct);

        var relative = $"/uploads/users/{userFolder}/signatures/{finalName}";
        return relative;
    }

    private string GetWebRoot()
    {
        var root = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        }

        Directory.CreateDirectory(root);
        return root;
    }

    private static string GuessExtension(string contentType)
    {
        return contentType?.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }
}