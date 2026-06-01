using Contracting.Shared.Dtos.ClientDtos.ProjectDtos;

namespace Contracting.Infrustructure.Inteface.client;

public interface IClientProjectService
{
    Task<List<GetClientProjectListItemDto>> GetClientProjectsAsync(CancellationToken cancellationToken = default);
}
