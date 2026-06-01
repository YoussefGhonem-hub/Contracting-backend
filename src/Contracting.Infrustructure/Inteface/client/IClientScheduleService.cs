using Contracting.Shared.Dtos.ClientDtos.ScheduleDtos;

namespace Contracting.Infrustructure.Inteface.client;

public interface IClientScheduleService
{
    /// <summary>
    /// Returns the Planning &amp; Schedule page data: summary card + monthly report files + timeline documents.
    /// Returns null if the project is not assigned to this client.
    /// </summary>
    Task<GetClientScheduleDto?> GetScheduleAsync(Guid projectId, CancellationToken cancellationToken = default);
}
