using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.TenderDtos;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.client;

public class ClientTenderService : IClientTenderService
{
    private readonly ApplicationDbContext _db;

    public ClientTenderService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<GetClientTenderDocumentDto>?> GetTenderDocumentsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id!.Value;

        var isClientProject = await _db.ClientProjects
            .AnyAsync(cp => cp.ProjectId == projectId
                         && cp.Client != null
                         && cp.Client.ApplicationUserId == userId,
                      cancellationToken);

        if (!isClientProject)
            return null;

        return await _db.TenderDocuments
            .Where(td => td.ProjectId == projectId)
            .OrderBy(td => td.CreatedDate)
            .Select(td => new GetClientTenderDocumentDto
            {
                Id = td.Id,
                Title = td.Title,
                FileName = td.FileName,
                Extension = td.Extension,
                FileSize = td.FileSize,
                Url = td.Url
            })
            .ToListAsync(cancellationToken);
    }
}
