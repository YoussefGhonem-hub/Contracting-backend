using Microsoft.AspNetCore.Http;
using Storage.AWS3.Models;

namespace Storage.AWS3.Services;
public interface IStorageService
{
    Task<StoredFile> Upload(IFormFile? file, CancellationToken cancellationToken = default);
    Task<List<StoredFile>?> UploadFiles(List<IFormFile>? file, CancellationToken cancellationToken = default);
    Task<string> UploadLocalVideoToS3Async(string localFilePath, CancellationToken cancellationToken = default);
    Task<bool> Delete(string key, CancellationToken cancellationToken = default);
    Task<DownloadedFile> DownloadFile(string key, CancellationToken cancellationToken = default);
    Task<string?> DownloadFileUrl(string? key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a time-limited pre-signed GET URL for the given object key so that
    /// clients can access private-bucket objects directly. Returns null if the key is
    /// empty or the URL could not be generated. This is a local operation (no network call).
    /// </summary>
    string? GetPreSignedUrl(string? key, TimeSpan? expiresIn = null);
    public Task<string> DownloadVideoFromS3ToLocalAsync(string keyOrUrl, string localFileName, CancellationToken cancellationToken = default); // ✅ NEW

}
