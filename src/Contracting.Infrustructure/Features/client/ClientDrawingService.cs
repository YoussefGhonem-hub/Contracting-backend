using Contracting.Domain.Common.Enums;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.DrawingDtos;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.client;

public class ClientDrawingService : IClientDrawingService
{
    private readonly ApplicationDbContext _db;

    public ClientDrawingService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<GetClientDrawingDto>?> GetDrawingsAsync(
        Guid projectId, string? type, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id!.Value;

        var isClientProject = await _db.ClientProjects
            .AnyAsync(cp => cp.ProjectId == projectId
                         && cp.Client != null
                         && cp.Client.ApplicationUserId == userId,
                      cancellationToken);

        if (!isClientProject)
            return null;

        var query = _db.ProjectDrawings.Where(d => d.ProjectId == projectId);

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<DrawingType>(type, ignoreCase: true, out var parsedType))
            query = query.Where(d => d.Type == parsedType);

        return await query
            .OrderByDescending(d => d.CreatedDate)
            .Select(d => new GetClientDrawingDto
            {
                Id = d.Id,
                Title = d.Title,
                Type = d.Type.ToString(),
                FileName = d.FileName,
                Extension = d.Extension,
                FileSize = d.FileSize,
                Url = d.Url,
                UploadedAt = d.CreatedDate
            })
            .ToListAsync(cancellationToken);
    }
}
