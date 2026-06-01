using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.TenderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Tender.Query.GetClientTenderDocuments;

public class GetClientTenderDocumentsQueryHandler : IRequestHandler<GetClientTenderDocumentsQuery, ErrorOr<List<GetClientTenderDocumentDto>>>
{
    private readonly IClientTenderService _service;

    public GetClientTenderDocumentsQueryHandler(IClientTenderService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<List<GetClientTenderDocumentDto>>> Handle(GetClientTenderDocumentsQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetTenderDocumentsAsync(request.ProjectId, cancellationToken);

        if (result is null)
            return Error.NotFound("Tender.ProjectNotFound", "Project not found or not assigned to this client.");

        return result;
    }
}
