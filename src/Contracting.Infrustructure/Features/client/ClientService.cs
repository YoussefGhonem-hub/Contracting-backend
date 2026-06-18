using Contracting.Domain.Entities.client;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.client;

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _db;

    public ClientService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<GetClientDto> CreateClientAsync(CreateClientDto dto, Guid userId)
    {
        var client = new Client
        {
            ApplicationUserId = userId,
            CompanyName = dto.CompanyName,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address
        };

        await _db.Clients.AddAsync(client);
        await _db.SaveChangesAsync();

        await ReplaceClientProjectsAsync(client.Id, dto.ProjectIds);

        return (await GetClientByIdAsync(client.Id))!;
    }

    public async Task<GetClientDto?> UpdateClientAsync(UpdateClientDto dto)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == dto.Id);
        if (client is null)
            return null;

        client.CompanyName = dto.CompanyName;
        client.PhoneNumber = dto.PhoneNumber;
        client.Address = dto.Address;

        await _db.SaveChangesAsync();

        await ReplaceClientProjectsAsync(client.Id, dto.ProjectIds);

        return await GetClientByIdAsync(client.Id);
    }

    public async Task<GenericResponse> DeleteClientAsync(Guid clientId)
    {
        var client = await _db.Clients
            .Include(c => c.ClientProjects)
            .FirstOrDefaultAsync(c => c.Id == clientId);

        if (client is null)
            return GenericResponse.FailureResult("Client not found.");

        var userId = client.ApplicationUserId;

        if (client.ClientProjects.Any())
            _db.ClientProjects.RemoveRange(client.ClientProjects);

        _db.Clients.Remove(client);

        // Remove the associated login user and its role assignments.
        var userRoles = _db.UserRoles.Where(ur => ur.UserId == userId);
        _db.UserRoles.RemoveRange(userRoles);

        var user = await _db.Users.FindAsync(userId);
        if (user is not null)
            _db.Users.Remove(user);

        await _db.SaveChangesAsync();

        return GenericResponse.SuccessResult("Client deleted successfully.");
    }

    public async Task<GetClientDto?> GetClientByIdAsync(Guid clientId)
    {
        var client = await _db.Clients
            .Include(c => c.ApplicationUser)
            .Include(c => c.ClientProjects)
                .ThenInclude(cp => cp.Project)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clientId);

        if (client is null)
            return null;

        var dto = MapToDto(client);
        dto.Roles = await GetUserRolesAsync(client.ApplicationUserId);
        return dto;
    }

    public async Task<PaginatedList<GetClientDto>> GetClientListAsync(BaseFilterDto filter)
    {
        var query = _db.Clients
            .Include(c => c.ApplicationUser)
            .Include(c => c.ClientProjects)
                .ThenInclude(cp => cp.Project)
            .AsNoTracking();

        query = string.IsNullOrWhiteSpace(filter.Sort)
            ? query.OrderByDescending(c => c.CreatedDate)
            : query.OrderByDynamic(filter.Sort, filter.Descending);

        var totalCount = await query.CountAsync();

        if (totalCount == 0)
            return new PaginatedList<GetClientDto>(new List<GetClientDto>(), 0, filter.PageIndex, filter.PageSize);

        var clients = await query
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var userIds = clients.Select(c => c.ApplicationUserId).ToList();

        var rolesByUser = await _db.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(_db.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new { ur.UserId, RoleId = r.Id, RoleName = r.Name })
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(x => new RoleDropDownDto { Id = x.RoleId, Name = x.RoleName }).ToList());

        var dtos = clients.Select(c =>
        {
            var dto = MapToDto(c);
            dto.Roles = rolesByUser.TryGetValue(c.ApplicationUserId, out var roles) ? roles : new List<RoleDropDownDto>();
            return dto;
        }).ToList();

        return new PaginatedList<GetClientDto>(dtos, totalCount, filter.PageIndex, filter.PageSize);
    }

    #region Helpers

    private async Task ReplaceClientProjectsAsync(Guid clientId, List<Guid> projectIds)
    {
        var existing = await _db.ClientProjects
            .Where(cp => cp.ClientId == clientId)
            .ToListAsync();

        if (existing.Any())
            _db.ClientProjects.RemoveRange(existing);

        if (projectIds is null || projectIds.Count == 0)
        {
            await _db.SaveChangesAsync();
            return;
        }

        var validIds = projectIds.Where(id => id != Guid.Empty).Distinct().ToList();
        var existingProjectIds = await _db.Projects
            .Where(p => validIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        var newLinks = existingProjectIds
            .Select(id => new ClientProject { ClientId = clientId, ProjectId = id });

        await _db.ClientProjects.AddRangeAsync(newLinks);
        await _db.SaveChangesAsync();
    }

    private async Task<List<RoleDropDownDto>> GetUserRolesAsync(Guid userId)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_db.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new RoleDropDownDto { Id = r.Id, Name = r.Name })
            .ToListAsync();
    }

    private static GetClientDto MapToDto(Client c) => new()
    {
        Id = c.Id,
        ApplicationUserId = c.ApplicationUserId,
        FullName = c.ApplicationUser?.FullName,
        Email = c.ApplicationUser?.Email,
        IsActive = c.ApplicationUser?.IsActive ?? false,
        CompanyName = c.CompanyName,
        PhoneNumber = c.PhoneNumber,
        Address = c.Address,
        Projects = c.ClientProjects
            .Where(cp => cp.Project != null)
            .Select(cp => new GetClientProjectItemDto
            {
                ProjectId = cp.ProjectId,
                nameEn = cp.Project.nameEn,
                nameAr = cp.Project.nameAr,
                Code = cp.Project.Code
            }).ToList()
    };

    #endregion
}
