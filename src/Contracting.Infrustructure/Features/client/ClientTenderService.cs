using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.TenderDtos;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Features.client;

public class ClientTenderService : IClientTenderService
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storage;

    public ClientTenderService(ApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<List<GetClientTenderDocumentDto>?> GetTenderDocumentsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        if (!await ClientProjectAccess.CanAccessProjectAsync(_db, projectId, cancellationToken))
            return null;

        var docs = await _db.TenderDocuments
            .Where(td => td.ProjectId == projectId)
            .OrderBy(td => td.CreatedDate)
            .Select(td => new
            {
                td.Id,
                td.Title,
                td.FileName,
                td.Extension,
                td.FileSize,
                td.Key,
                td.Url
            })
            .ToListAsync(cancellationToken);

        // The DB stores raw (private-bucket) S3 URLs which return Access Denied.
        // Hand back a time-limited pre-signed URL generated from the object key instead.
        return docs.Select(td => new GetClientTenderDocumentDto
        {
            Id = td.Id,
            Title = td.Title,
            FileName = td.FileName,
            Extension = td.Extension,
            FileSize = td.FileSize,
            Url = _storage.GetPreSignedUrl(td.Key) ?? td.Url
        }).ToList();
    }
}
