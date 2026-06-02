using Contracting.Shared.Dtos.ClientDtos.DrawingDtos;

namespace Contracting.Infrustructure.Inteface.client;

public interface IClientDrawingService
{
    Task<List<GetClientDrawingDto>?> GetDrawingsAsync(Guid projectId, string? type, CancellationToken cancellationToken = default);
}
