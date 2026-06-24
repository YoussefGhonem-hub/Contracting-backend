using Contracting.Shared.Storage;
using Microsoft.AspNetCore.Http;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Files;

/// <summary>
/// IFileStorage backed by AWS S3. Uses a key prefix ("subfolder") so that
/// GetPreSignedUrl(key) in client-facing services can find the file.
/// Replaces LocalFileStorage in production to fix the NoSuchKey download error.
/// </summary>
public class S3FileStorage : IFileStorage
{
    private readonly IStorageService _storage;

    public S3FileStorage(IStorageService storage)
    {
        _storage = storage;
    }

    public async Task<string> SaveAsync(IFormFile file, string subFolder, CancellationToken ct)
    {
        var stored = await _storage.UploadWithKeyPrefix(file, subFolder, ct);
        return stored.Key;
    }

    public async Task DeleteAsync(string key, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(key))
            await _storage.Delete(key, ct);
    }

    public async Task DeleteManyAsync(IEnumerable<string> keys, CancellationToken ct)
    {
        foreach (var key in keys)
            await DeleteAsync(key, ct);
    }

    public async Task<string> SaveUserAvatarAsync(Guid userId, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var tempFile = new MemoryFormFile(stream, fileName, contentType);
        var stored = await _storage.UploadWithKeyPrefix(tempFile, $"uploads/users/{userId:N}", ct);
        return stored.Key;
    }

    public async Task<string> SaveUserSignatureAsync(Guid userId, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var tempFile = new MemoryFormFile(stream, fileName, contentType);
        var stored = await _storage.UploadWithKeyPrefix(tempFile, $"uploads/users/{userId:N}/signatures", ct);
        return stored.Key;
    }

    // Minimal IFormFile wrapper so we can reuse UploadWithKeyPrefix for Stream inputs.
    private sealed class MemoryFormFile : IFormFile
    {
        private readonly Stream _stream;
        public MemoryFormFile(Stream stream, string fileName, string contentType)
        {
            _stream = stream;
            FileName = fileName;
            ContentType = contentType;
            Name = fileName;
            Length = stream.Length;
        }
        public string ContentType { get; }
        public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{FileName}\"";
        public IHeaderDictionary Headers => new HeaderDictionary();
        public long Length { get; }
        public string Name { get; }
        public string FileName { get; }
        public Stream OpenReadStream() => _stream;
        public void CopyTo(Stream target) => _stream.CopyTo(target);
        public Task CopyToAsync(Stream target, CancellationToken ct = default) => _stream.CopyToAsync(target, ct);
    }
}
