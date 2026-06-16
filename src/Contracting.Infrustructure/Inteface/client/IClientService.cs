using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;

namespace Contracting.Infrustructure.Inteface.client;

public interface IClientService
{
    /// <summary>Creates the client profile (and project links) for an already-created login user.</summary>
    Task<GetClientDto> CreateClientAsync(CreateClientDto dto, Guid userId);

    /// <summary>Updates the client profile and its project links. Returns null if not found.</summary>
    Task<GetClientDto?> UpdateClientAsync(UpdateClientDto dto);

    /// <summary>Deletes the client profile, its project links, and the associated login user.</summary>
    Task<GenericResponse> DeleteClientAsync(Guid clientId);

    Task<GetClientDto?> GetClientByIdAsync(Guid clientId);

    Task<PaginatedList<GetClientDto>> GetClientListAsync(BaseFilterDto filter);
}
