using Contracting.Shared.Dtos.ClientDtos.TenderDtos;

namespace Contracting.Infrustructure.Inteface.client;

public interface IClientTenderService
{
    /// <summary>
    /// Returns all tender documents for the given project.
    /// Returns null if the project is not assigned to this client.
    /// </summary>
    Task<List<GetClientTenderDocumentDto>?> GetTenderDocumentsAsync(Guid projectId, CancellationToken cancellationToken = default);
}
