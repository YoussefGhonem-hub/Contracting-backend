using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.InvoiceDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Invoice.Query.GetClientInvoices;

public class GetClientInvoicesQueryHandler : IRequestHandler<GetClientInvoicesQuery, ErrorOr<GetClientInvoicesDto>>
{
    private readonly IClientInvoiceService _service;

    public GetClientInvoicesQueryHandler(IClientInvoiceService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<GetClientInvoicesDto>> Handle(GetClientInvoicesQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetClientInvoicesAsync(request.ProjectId, request.Status, cancellationToken);

        if (result is null)
            return Error.NotFound("Invoice.ProjectNotFound", "Project not found or not assigned to this client.");

        return result;
    }
}
