using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.ClientContent.Command.UpdateTenderDocument;

public class UpdateTenderDocumentCommandHandler : IRequestHandler<UpdateTenderDocumentCommand, ErrorOr<UpdatedTenderDocumentDto>>
{
    private readonly IClientContentService _service;

    public UpdateTenderDocumentCommandHandler(IClientContentService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<UpdatedTenderDocumentDto>> Handle(UpdateTenderDocumentCommand request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateTenderDocumentAsync(request.TenderId, request.Title, request.File, cancellationToken);

        if (result is null)
            return Error.NotFound("TenderDocument.NotFound", "Tender document not found.");

        return result;
    }
}
