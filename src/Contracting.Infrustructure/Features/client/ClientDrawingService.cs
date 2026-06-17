using Contracting.Domain.Common.Enums;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.DrawingDtos;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Features.client;

public class ClientDrawingService : IClientDrawingService
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storage;

    public ClientDrawingService(ApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<List<GetClientDrawingDto>?> GetDrawingsAsync(
        Guid projectId, string? type, CancellationToken cancellationToken = default)
    {
        if (!await ClientProjectAccess.CanAccessProjectAsync(_db, projectId, cancellationToken))
            return null;

        var query = _db.ProjectDrawings.Where(d => d.ProjectId == projectId);

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<DrawingType>(type, ignoreCase: true, out var parsedType))
            query = query.Where(d => d.Type == parsedType);

        var drawings = await query
            .OrderByDescending(d => d.CreatedDate)
            .Select(d => new
            {
                d.Id,
                d.Title,
                d.Type,
                d.FileName,
                d.Extension,
                d.FileSize,
                d.Key,
                d.Url,
                d.CreatedDate
            })
            .ToListAsync(cancellationToken);

        // Stored URLs point at a private bucket (Access Denied); return pre-signed URLs.
        return drawings.Select(d => new GetClientDrawingDto
        {
            Id = d.Id,
            Title = d.Title,
            Type = d.Type.ToString(),
            FileName = d.FileName,
            Extension = d.Extension,
            FileSize = d.FileSize,
            Url = _storage.GetPreSignedUrl(d.Key) ?? d.Url,
            UploadedAt = d.CreatedDate
        }).ToList();
    }
}
