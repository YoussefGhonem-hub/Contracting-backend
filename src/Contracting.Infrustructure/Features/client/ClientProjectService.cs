using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.ProjectDtos;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.client;

public class ClientProjectService : IClientProjectService
{
    private readonly ApplicationDbContext _db;

    public ClientProjectService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<GetClientProjectListItemDto>> GetClientProjectsAsync(CancellationToken cancellationToken = default)
    {
        // Clients see only the projects assigned to them; staff see every project.
        if (ClientProjectAccess.IsClient)
        {
            var userId = CurrentUser.Id!.Value;

            return await _db.ClientProjects
                .Where(cp => cp.Client != null && cp.Client.ApplicationUserId == userId)
                .Select(cp => new GetClientProjectListItemDto
                {
                    Id = cp.Project!.Id,
                    NameEn = cp.Project.nameEn,
                    NameAr = cp.Project.nameAr,
                    Location = cp.Project.location,
                    ImageUrl = cp.Project.imageUrl,
                    Area = cp.Project.Area,
                    StartDate = cp.Project.StartDate,
                    ProjectStatus = cp.Project.ProjectStatus,
                    Code = cp.Project.Code
                })
                .ToListAsync(cancellationToken);
        }

        return await _db.Projects
            .Where(p => !p.IsDeleted)
            .Select(p => new GetClientProjectListItemDto
            {
                Id = p.Id,
                NameEn = p.nameEn,
                NameAr = p.nameAr,
                Location = p.location,
                ImageUrl = p.imageUrl,
                Area = p.Area,
                StartDate = p.StartDate,
                ProjectStatus = p.ProjectStatus,
                Code = p.Code
            })
            .ToListAsync(cancellationToken);
    }
}
