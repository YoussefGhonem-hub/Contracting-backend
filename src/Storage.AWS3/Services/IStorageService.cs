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
    public Task<string> DownloadVideoFromS3ToLocalAsync(string keyOrUrl, string localFileName, CancellationToken cancellationToken = default); // ✅ NEW

}
